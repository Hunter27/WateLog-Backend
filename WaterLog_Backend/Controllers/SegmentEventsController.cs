using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
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

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SegmentEventsEntry>>> GetAllSegmentEvents()
        {
            return await _db.SegmentEvents.ToListAsync();
        }

        // GET api/values/5
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

        // POST api/values
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
            Procedures proc = new Procedures(_db, _config);
            var ret = await proc.CalculatePeriodWastageAsync(Procedures.Period.Daily);
            return ret.FirstOrDefault();
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

        //Routing to get all currently opened events
        [Route("GetAlerts")]
        public async Task<List<GetAlerts>> GetAlerts()
        {
            try
            {
                //Get all leaks first
                var leaks = await _db.SegmentLeaks.Where(a => a.ResolvedStatus == "unresolved").ToListAsync();
                if (leaks != null)
                {
                    List<GetAlerts> alerts = new List<GetAlerts>();
                    var proc = new Procedures(_db, _config);
                    foreach (SegmentLeaksEntry entry in leaks)
                    {
                        //Find Cost
                        var cost = proc.calculatePerHourCost(entry);
                        //Find Litre Usage
                        //TODO: Call TotalLitres Used(Dependent on Usage/Cost Feature Branch)
                        var litresUsed = proc.calculateTotaLitres(entry);
                        alerts.Add
                        (
                            new GetAlerts
                            (
                                entry.OriginalTimeStamp,
                                "Segment",
                                entry.SegmentsId,
                                "leak",
                                cost,
                                entry.Severity,
                                litresUsed,
                                0.0
                             )
                        );
                    }

                    //Find All Sensors that are faulty
                    var faultySensors = await _db.Monitors.Where(a => a.Status == "faulty").ToListAsync();
                    if (faultySensors != null)
                    {
                        foreach (MonitorsEntry entry in faultySensors)
                        {
                            //Get latest faulty sensor
                            var sensor = await _db.Readings.Where(a => a.Value == 0)
                                        .OrderByDescending(a => a.TimesStamp)
                                        .FirstOrDefaultAsync();

                            if (entry.Id == sensor.MonitorsId)
                            {
                                //Have the correct information
                                alerts.Add
                                (
                                    new GetAlerts
                                    (
                                        sensor.TimesStamp,
                                        "Sensor",
                                        sensor.MonitorsId,
                                        "faulty",
                                        0.0,
                                        "severe",
                                        0.0,
                                        0.0
                                     )
                                 );
                            }
                        }
                    }
                    return alerts;
                }
                throw new Exception("ERROR : Null SegmentLeaks");
            }
            catch (Exception error)
            {
                throw error;
            }
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
