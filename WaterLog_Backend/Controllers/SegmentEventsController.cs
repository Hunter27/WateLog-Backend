using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using NUnit.Framework;
using WaterLog_Backend.Models;

namespace WaterLog_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SegmentEventsController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public SegmentEventsController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // GET api/events
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SegmentEventsEntry>>> GetAllSegmentEvents()
        {
            return await _db.SegmentEvents.ToListAsync();
        }

        // GET api/eventsById/
        [HttpGet("{id}")]
        public async Task<ActionResult<SegmentEventsEntry>> GetSegmentById(int id)
        {
            var segment = await _db.SegmentEvents.FindAsync(id);

            if (segment == null)
            {
                return NotFound();
            }
            return segment;
        }

        //POST api/events
        [HttpPost]
        public async Task AddSegmentEvent([FromBody] SegmentEventsEntry value)
        {
            try
            {
                await _db.SegmentEvents.AddAsync(value);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("error", e);
            }
        }

        [Route("dailywastage")]
        public async Task<DataPoints<DateTime, double>> GetDailyWastageGraphData()
        {
            var proc = new Procedures(_db, _config);
            var dailyWastage = await proc.CalculatePeriodWastageAsync(Procedures.Period.Daily);
            var graphData = new DataPoints<DateTime, double>();
            var dummyHours = new List<DateTime>();
            for (int i = 0; i < 24; i++)
            {
                DateTime tempTime = DateTime.Now;
                DateTime dateWithSpecificHour = new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, i, 0, 0);
                var perHourData = dailyWastage.FirstOrDefault().dataPoints
                    .Where(dataPoint => dataPoint.x.Hour == dateWithSpecificHour.Hour);
                double average = 0;
                int length = perHourData.Count();
                for (int j = 0; j < length; j++)
                {
                    average += perHourData.ElementAt(j).y;
                }
                average = average / length;
                graphData.AddPoint(dateWithSpecificHour, average);
            }

            return graphData;
        }

        [Route("monthlywastage")]
        public async Task<DataPoints<DateTime, double>> GetMonthlyWastageGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.CalculatePeriodWastageAsync(Procedures.Period.Monthly);
            return ret.FirstOrDefault();
        }

        [Route("seasonallywastage")]
        public async Task<DataPoints<DateTime, double>[]> GetSeasonallyWastageGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            return await proc.CalculatePeriodWastageAsync(Procedures.Period.Seasonally);
        }

        //Gets Alerts only loading max 10 elements at a time
        [Route("GetAlerts/{id}")]
        public async Task<List<GetAlerts>> GetAlertsByPage(int id = 1)
        {
            List<GetAlerts> ListOfAlerts = new List<GetAlerts>();
            //Get Entries from SegmentLeaks
            var leaksQuery = await _db.SegmentLeaks.OrderByDescending(a => a.OriginalTimeStamp)
                                      .OrderByDescending(b => b.ResolvedStatus).Skip((id - 1) * Globals.NumberItems)
                                      .Take(Globals.NumberItems).ToListAsync();

            if (leaksQuery.Count != 0)
            {
                leaksQuery = leaksQuery.OrderByDescending(a => a.OriginalTimeStamp)
                    .OrderByDescending(a => a.ResolvedStatus).ToList();

                var proc = new Procedures(_db, _config);

                foreach (SegmentLeaksEntry entry in leaksQuery)
                {
                    double totalSystemLitres = await proc.CalculateTotalUsageLitres(entry),
                            litresUsed = await proc.CalculateTotalWastageLitres(entry),
                            perhourwastagelitre = await proc.CalculatePerHourWastageLitre(entry),
                            cost = await proc.CalculatePerHourWastageCost(entry);

                    //Find Litre Usage
                    ListOfAlerts.Add
                     (
                         new GetAlerts
                         (
                             entry.OriginalTimeStamp,
                             (entry
                             .LatestTimeStamp
                             .Subtract(entry
                             .OriginalTimeStamp) < TimeSpan.Zero ? TimeSpan.Zero : entry
                             .LatestTimeStamp.
                             Subtract(entry
                             .OriginalTimeStamp)),
                             COMPONENT_TYPES.SEGMENT,
                             entry.SegmentsId,
                             Globals.Leak,
                             cost,
                             perhourwastagelitre,
                             entry.Severity,
                             litresUsed,
                             totalSystemLitres,
                             entry.ResolvedStatus
                          )
                     );
                }
                //find empty tanks
                var emptyTank = await _db.TankReadings.OrderByDescending(a => a.TimeStamp)
                                      .OrderByDescending(b => b.PercentageLevel == 0).Skip((id - 1) * Globals.NumberItems)
                                      .Take(Globals.NumberItems).ToListAsync();

                if (emptyTank.Count != 0)
                {
                    foreach (TankReadingsEntry entry in emptyTank)
                    {

                        var tankInfo = await _db.Monitors.Where(a => a.Id == entry.TankMonitorsId).FirstOrDefaultAsync();
                        var latestReading = await _db.Readings.Where(a => a.MonitorsId == entry.TankMonitorsId)
                            .OrderByDescending(a => a.TimesStamp).FirstOrDefaultAsync();

                        ListOfAlerts.Add
                        (
                            new GetAlerts
                            (
                                entry.TimeStamp,
                                TimeSpan.Zero,
                                COMPONENT_TYPES.TANK,
                                entry.TankMonitorsId,
                                Globals.Empty,
                                0.0,
                                0.0,
                                "High",
                                latestReading.Value,
                                tankInfo.Max_flow,
                                entry.PercentageLevel == 0 ? EnumResolveStatus.UNRESOLVED : EnumResolveStatus.RESOLVED
                             )
                         );
                    }
                    //Find All Sensors that are faulty
                    var faultySensors = await _db.SensorHistory.OrderByDescending(a => a.FaultDate)
                    .OrderByDescending(b => b.SensorResolved)
                    .Skip((id - 1) * Globals.NumberItems).Take(Globals.NumberItems).ToListAsync();

                    if (faultySensors.Count != 0)
                    {
                        foreach (SensorHistoryEntry entry in faultySensors)
                        {

                            var sensorInfo = await _db.Monitors.Where(a => a.Id == entry.SensorId).FirstOrDefaultAsync();
                            var latestReading = await _db.Readings.Where(a => a.MonitorsId == entry.SensorId)
                                .OrderByDescending(a => a.TimesStamp).FirstOrDefaultAsync();

                            ListOfAlerts.Add
                            (
                                new GetAlerts
                                (
                                    entry.FaultDate,
                                    (entry
                                    .AttendedDate
                                    .Subtract(entry.AttendedDate) < TimeSpan.Zero ? TimeSpan.Zero : entry
                                    .FaultDate.
                                    Subtract(entry
                                    .AttendedDate)),
                                    COMPONENT_TYPES.SENSOR,
                                    entry.SensorId,
                                    Globals.Faulty,
                                    0.0,
                                    0.0,
                                    "High",
                                    latestReading.Value,
                                    sensorInfo.Max_flow,
                                    entry.SensorResolved
                                 )
                             );
                        }
                    }
                }
            }
            return ListOfAlerts.OrderByDescending(a => a.Date).OrderByDescending(a => a.Status).ToList();
        }

        [HttpPost("GetAlertsFilter")]
        public async Task<List<GetAlerts>> GetAlertsByPage([FromBody] Filter filter)
        {
            //Get all alerts
            var alerts = await this.GetAlerts();
            var filteredAlerts = new List<GetAlerts>();
            bool onlySeverity = true;
            //filter by Segments
            if (filter.Segment != 0)
            {
                onlySeverity = false;
                var segmentData = alerts
                    .Where(alert => alert.EntityName == COMPONENT_TYPES.SEGMENT && alert.EntityId == filter.Segment)
                    .ToList();
                filteredAlerts.AddRange(segmentData);
            }

            if (filter.SensorType != 0)
            {
                var sensorData = new List<GetAlerts>();
                if (filter.SensorType == 1)
                {
                    onlySeverity = false;
                    sensorData = alerts
                        .Where(alert => alert.EntityName == COMPONENT_TYPES.SENSOR && alert.EntityId == filter.SensorId)
                        .ToList();
                }
                else if (filter.SensorType == 2)
                {
                    onlySeverity = false;
                    sensorData = alerts
                        .Where(alert => alert.EntityName == COMPONENT_TYPES.TANK && alert.EntityId == filter.SensorId)
                        .ToList();
                }
                filteredAlerts.AddRange(sensorData);
            }

            if (onlySeverity)
            {
                return alerts
                     .Where(alert => alert.Severity.ToLower() == filter.Severity.ToLower())
                     .ToList();
            }
            else if (filteredAlerts.Count != 0)
            {
                return filteredAlerts
                     .Where(alert => alert.Severity.ToLower() == filter.Severity.ToLower())
                     .ToList();
            }
            return filteredAlerts;
        }
        //Routing to get all currently opened events
        [Route("GetAlerts")]
        public async Task<List<GetAlerts>> GetAlerts()

        {
            try
            {
                //Get all leaks first
                var leaks = await _db.SegmentLeaks.ToListAsync();
                leaks = leaks.OrderByDescending(a => a.OriginalTimeStamp).OrderByDescending(a => a.ResolvedStatus).ToList();
                if (leaks.Count != 0)
                {
                    List<GetAlerts> alerts = new List<GetAlerts>();
                    var proc = new Procedures(_db, _config);
                    foreach (SegmentLeaksEntry entry in leaks)
                    {
                        double totalSystemLitres = -1.0, litresUsed = -1.0,
                            perhourwastagelitre = await proc.CalculatePerHourWastageLitre(entry),
                            cost = await proc.CalculatePerHourWastageCost(entry);

                        //Find Cost
                        if (entry.ResolvedStatus == EnumResolveStatus.UNRESOLVED)
                        {
                            totalSystemLitres = await proc.CalculateTotalUsageLitres(entry);
                            litresUsed = await proc.CalculateTotalWastageLitres(entry);
                        }

                        //Find Litre Usage
                        alerts.Add
                        (
                            new GetAlerts
                            (
                                entry.OriginalTimeStamp,
                                (entry
                                 .LatestTimeStamp
                                 .Subtract(entry
                                 .OriginalTimeStamp) < TimeSpan.Zero ? TimeSpan.Zero : entry
                                 .LatestTimeStamp.
                                 Subtract(entry
                                 .OriginalTimeStamp)),
                                 COMPONENT_TYPES.SEGMENT,
                                 entry.SegmentsId,
                                 Globals.Leak,
                                 cost,
                                 perhourwastagelitre,
                                 entry.Severity,
                                 litresUsed,
                                 totalSystemLitres,
                                 entry.ResolvedStatus
                             )
                        );
                    }

                    //Find All Sensors that are faulty
                    var faultySensors = await _db.SensorHistory.ToListAsync();
                    if (faultySensors.Count != 0)
                    {
                        foreach (SensorHistoryEntry entry in faultySensors)
                        {
                            var sensorInfo = await _db.Monitors.Where(a => a.Id == entry.SensorId).FirstOrDefaultAsync();
                            var latestReading = await _db.Readings.Where(a => a.MonitorsId == entry.SensorId)
                                .OrderByDescending(a => a.TimesStamp).FirstOrDefaultAsync();

                            alerts.Add
                            (
                                new GetAlerts
                                (
                                    entry.FaultDate,
                                     (entry
                                    .AttendedDate
                                    .Subtract(entry.AttendedDate) < TimeSpan.Zero ? TimeSpan.Zero : entry
                                    .FaultDate.
                                    Subtract(entry
                                    .AttendedDate)),
                                    COMPONENT_TYPES.SENSOR,
                                    entry.SensorId,
                                    Globals.Faulty,
                                    0.0,
                                    0.0,
                                    "High",
                                    latestReading.Value,
                                    sensorInfo.Max_flow,
                                    entry.SensorResolved
                                 )
                             );
                        }
                    }
                    return (alerts.OrderByDescending(a => a.Date).ToList());
                }
                throw new Exception("ERROR : Null SegmentLeaks");
            }
            catch (Exception error)
            {
                throw error;
            }
        }

        [Route("dailyUsage")]
        public async Task<DataPoints<DateTime, double>> GetDailyUsgaeGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.SummaryPeriodUsageAsync(Procedures.Period.Daily);
            var outV = new DataPoints<DateTime, double>();
            var dummyHours = new List<DateTime>();
            for (int i = 0; i < 24; i++)
            {
                DateTime tempTime = DateTime.Now;
                DateTime returnV = new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, i, 0, 0);
                for (int j = 0; j < ret.Length; j++)
                {
                    if (ret.ElementAt(j).dataPoints.Count < 1)
                    {
                        continue;
                    }
                    var dateValue = ret.ElementAt(j).getvalueT();
                    var reading = ret.ElementAt(j).getValueY();
                    if (dateValue.ElementAt(0).Hour == returnV.Hour)
                    {
                        outV.AddPoint(dateValue.ElementAt(0), reading.ElementAt(0));
                    }
                    else
                    {
                        outV.AddPoint(dateValue.ElementAt(0), 0);
                    }
                }
            }

            return outV;
        }
        [Route("monthlyUsage")]
        public async Task<DataPoints<DateTime, double>> GetMonthlyUsageGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.SummaryPeriodUsageAsync(Procedures.Period.Monthly);
            return ret.FirstOrDefault();
        }
        [Route("seasonallyUsage")]
        public async Task<DataPoints<DateTime, double>[]> GetSeasonallyUsgaeGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            return await proc.SummaryPeriodUsageAsync(Procedures.Period.Seasonally);
        }
        [Route("dailyCost")]
        public async Task<DataPoints<DateTime, double>> GetDailyCostGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.SummaryPeriodCostsAsync(Procedures.Period.Daily);
            var outV = new DataPoints<DateTime, double>();
            var dummyHours = new List<DateTime>();
            for (int i = 0; i < 24; i++)
            {
                DateTime tempTime = DateTime.Now;
                DateTime returnV = new DateTime(tempTime.Year, tempTime.Month, tempTime.Day, i, 0, 0);
                for (int j = 0; j < ret.Count(); j++)
                {
                    if (ret.ElementAt(j).dataPoints.Count < 1)
                    {
                        continue;
                    }
                    var dateValue = ret.ElementAt(j).getvalueT();
                    var reading = ret.ElementAt(j).getValueY();
                    if (dateValue.ElementAt(0).Hour == returnV.Hour)
                    {
                        outV.AddPoint(dateValue.ElementAt(0), reading.ElementAt(0));
                    }
                    else
                    {
                        outV.AddPoint(dateValue.ElementAt(0), 0);
                    }
                }
            }

            return outV;
        }
        [Route("monthlyCost")]
        public async Task<DataPoints<DateTime, double>> GetMonthlyCostGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.SummaryPeriodCostsAsync(Procedures.Period.Monthly);
            return ret.FirstOrDefault();
        }
        [Route("seasonallyCost")]
        public async Task<DataPoints<String, double>> GetSeasonallyUsageGraphData()
        {
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.SummaryPeriodCostsSeasonAsync();
            return ret;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task UpdateSegmentEvent(int id, [FromBody] SegmentEventsEntry value)
        {
            try
            {
                var old = await _db.SegmentEvents.FindAsync(id);
                _db.Entry(old).CurrentValues.SetValues(value);
                await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("error", e);
            }
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task DeleteSegmentEvent(int id)
        {
            var entry = await _db.SegmentEvents.FindAsync(id);
            _db.SegmentEvents.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}
