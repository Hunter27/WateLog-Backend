using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailNotifications;
using WaterLog_Backend.Models;

namespace WaterLog_Backend
{
    public class Procedures
    {
        DatabaseContext _db;
        IConfiguration _config;
        public Procedures() {

        }
        public Procedures(DatabaseContext db,IConfiguration cfg)
        {
            _db = db;
            _config = cfg;
        }

        public async Task TriggerInsert(ReadingsEntry value)
        {
            SegmentsEntry segment = await _db.Segments.Where(ins => ins.SenseIDIn == value.MonitorsId).SingleOrDefaultAsync();
            if (segment == null)
            {
                return;
            }

            int segmentInid = -1;
            int segmentOutid = -1;
            int segmentid = -1;

            segmentInid = segment.SenseIDIn;
            segmentOutid = segment.SenseIDOut;
            segmentid = segment.Id;

            ReadingsEntry reading1 = await _db.Readings
            .Where(r => r.MonitorsId == segmentInid)
            .OrderByDescending(r => r.TimesStamp)
            .FirstAsync();

            ReadingsEntry reading2 = await _db.Readings
            .Where(re => re.MonitorsId == segmentOutid)
            .OrderByDescending(re => re.TimesStamp)
            .FirstAsync();

            if (IsLeakage(reading1.Value, reading2.Value))
            {
                //Updateleakagestatus
                IEnumerable<SegmentLeaksEntry> allLeaks = _db.SegmentLeaks;
                if (allLeaks.Any(leak => leak.SegmentsId == segmentid))
                {
                    SegmentLeaksEntry latestEntry = await _db.SegmentLeaks
                    .Where(leak => leak.SegmentsId == segmentid && leak.ResolvedStatus == "unresolved")
                    .OrderByDescending(lk => lk.LatestTimeStamp)
                    .FirstAsync();
                    //Check in SegmentEntry if latest event related to entry has been resolved.
                    if (latestEntry != null)
                    {
                        SegmentEventsEntry entry = (await _db.SegmentEvents
                        .Where(leak => leak.SegmentsId == segmentid)
                        .OrderByDescending(lks => lks.TimeStamp)
                        .FirstAsync());

                        if (entry.EventType == "leak")
                        {
                            await UpdateSegmentLeaksAsync(latestEntry.Id, segmentid, await CalculateSeverity(latestEntry), latestEntry.OriginalTimeStamp, entry.TimeStamp, "unresolved",latestEntry.LastNotificationDate);
                        }
                    }
                }
                else
                {
                    //Normal Add
                    await CreateSegmentLeaksAsync(segmentid, "unresolved");
                    //Call an initial email
                    //Get recipients.
                    var mailing = await _db.MailingList.Where(a => a.ListGroup == "tier2").ToListAsync();
                    if (mailing.Count > 0)
                    {
                        string[] template = populateEmail(segmentid);
                        Email email = new Email(template, _config);
                        Recipient[] mailers = new Recipient[(mailing.Count - 1)];
                        int countForMailers = 0;
                        foreach(var rec in mailing)
                        {
                            mailers[countForMailers] = new Recipient(rec.Address, (rec.Name + " " + rec.Surname));
                        }
                        email.SendMail(mailers);
                    }
                }
            }
            else
            {
                //Updatewithoutleakagestatus
                await UpdateSegmentsEventAsync(segmentid, "normal", reading1.Value, reading2.Value);
            }
        }

        public async Task<string> CalculateSeverity(SegmentLeaksEntry entry)
        {
            
            return CalculateSeverityGivenValue(await CalculateTotalWastageLitres(entry));

            
        }

        public string CalculateSeverityGivenValue(double value)
        {
            if (value >= 100)
            {
                return "High";
            }
            else if (value < 50)
            {
                return "Low";
            }
            else
            {
                return "Medium";
            }
        }

        public async Task CreateSegmentLeaksAsync(int segId, string resolvedStatus)
        {
            SegmentLeaksEntry entry = new SegmentLeaksEntry();
            entry.SegmentsId = segId;
            entry.LatestTimeStamp = DateTime.Now;
            entry.OriginalTimeStamp = DateTime.Now;
            entry.LatestTimeStamp = DateTime.Now;
            entry.ResolvedStatus = resolvedStatus;
            entry.Severity = await CalculateSeverity(entry);
            await _db.SegmentLeaks.AddAsync(entry);
            await _db.SaveChangesAsync();

        }
        
        public async Task UpdateSegmentLeaksAsync(int leakId, int segId, string severity, DateTime original, DateTime updated, string resolvedStatus,DateTime lastEmail)
        {
            try
            {
                bool toSend = false;
                if ((DateTime.Now - lastEmail).Days >= 1)
                {
                    toSend = true;
                    lastEmail = DateTime.Now;
                }
                SegmentLeaksEntry entry = new SegmentLeaksEntry();
                entry.SegmentsId = segId;
                entry.Severity = severity;
                entry.OriginalTimeStamp = original;
                entry.LatestTimeStamp = updated;
                entry.ResolvedStatus = resolvedStatus;
                entry.Id = leakId;
                entry.LastNotificationDate = lastEmail;
                var old = await _db.SegmentLeaks
                .FindAsync(leakId);

                _db.Entry(old)
                .CurrentValues
                .SetValues(entry);

                await _db.SaveChangesAsync();
                if (toSend)
                {
                    //We need to send an email to update that notification is old.
                    string[] template = populateEmail(segId);
                    Email email = new Email(template, _config);
                    if ((DateTime.Now - lastEmail).Days >= 4)
                    {
                        //Send to tier 1
                        email.SendMail(await GetTier1ListAsync());
                    }
                    else
                    {
                        //Send to tier 2
                        email.SendMail(await GetTier2ListAsync());
                    }
                }
            }
            catch(Exception error)
            {
                throw new Exception(error.Message);
            }
        }

        //Returns the Addresses that exist for tier 1 recipients
        public async Task<Recipient[]> GetTier1ListAsync()
        {
            var mailing = await _db.MailingList.Where(a => a.ListGroup == "tier1").ToListAsync();
            if (mailing.Count > 0)
            {
                Recipient[] mailers = new Recipient[(mailing.Count - 1)];
                int countForMailers = 0;
                foreach (var rec in mailing)
                {
                    mailers[countForMailers] = new Recipient(rec.Address, (rec.Name + " " + rec.Surname));
                }
                return mailers;
            }
            else
            {
                return null;
            }
        }

        //Return the addresses that exist for tier 2 recipients
        public async Task<Recipient[]> GetTier2ListAsync()
        {
            var mailing = await _db.MailingList.Where(a => a.ListGroup == "tier2").ToListAsync();
            if (mailing.Count > 0)
            {
                Recipient[] mailers = new Recipient[(mailing.Count - 1)];
                int countForMailers = 0;
                foreach (var rec in mailing)
                {
                    mailers[countForMailers] = new Recipient(rec.Address, (rec.Name + " " + rec.Surname));
                }
                return mailers;
            }
            else
            {
                return null;
            }
        }

        public async Task UpdateSegmentsEventAsync(int id, string status, double inv, double outv)
        {
            SegmentEventsEntry entry = new SegmentEventsEntry();
            entry.TimeStamp = DateTime.Now;
            entry.SegmentsId = id;
            entry.FlowIn = inv;
            entry.FlowOut = outv;
            entry.EventType = status;
            await _db.SegmentEvents
            .AddAsync(entry);
            await _db.SaveChangesAsync();
        }


        public Boolean IsLeakage(double first, double second)
        {
            double margin = 2;
            if ((first - second) > margin)
            {
                return true;
            }
            return false;
        }

        public string[] populateEmail(int sectionid)
        {
            try
            {
                var leaks = _db.SegmentLeaks;
                var leak = leaks
                .Where(sudo => sudo.SegmentsId == sectionid)
                .Single();

                string[] template = 
                {
                 "Segment " + leak.SegmentsId,
                  GetSegmentStatus(leak.SegmentsId),
                  leak.Severity,
                  GetLeakPeriod(leak),
                  Math.Round(CalculateTotalCost(leak)).ToString(),
                  Math.Round(CalculatePerHourWastageCost(leak)).ToString(),
                  Math.Round(CalculatePerHourWastageLitre(leak)).ToString(),
                  BuildUrl(leak.Id)
                };
                return template;
            }
            catch(Exception error)

            {
                throw error;
            }  
        }

        private string GetLeakPeriod(SegmentLeaksEntry leak)
        {
                return ((Math.Round((leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalDays,1)).ToString());
        }

        public double CalculateTotalCost(SegmentLeaksEntry leak)
        {
            var list = _db.SegmentEvents;
            var entry = list
            .Where(inlist => inlist.SegmentsId == leak.SegmentsId)
            .Last();

            var timebetween = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours;
            if (timebetween < 1)
            {
                return CalculatePerHourWastageCost(leak)/60;
            }
            else
            {
                timebetween = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours;
            }
            var perhour = CalculatePerHourWastageCost(leak);
            var total =  (timebetween * perhour);

            if(total < 0)
            {
                throw new Exception("ERROR : TotalCost Less Than 0");
            }
            else
            {
                return total;
            }
        }

        private string BuildUrl(int segmentId)
        {
            return "https://iot.retrotest.co.za/alert/segment/" + segmentId;
        }

        public double CalculatePerHourWastageCost(SegmentLeaksEntry leak)
        {
            var list = _db.SegmentEvents;
            var entry = list
            .Where(inlist => inlist.SegmentsId == leak.SegmentsId)
            .Last();

            double currentTariff = 37.5;
            double usageperpoll = (entry.FlowIn - entry.FlowOut);
            var totalPH = (usageperpoll * currentTariff);
            if(totalPH < 0)
            {
                throw new Exception("ERROR : Per Hour Cost Less Than 0");
            }
            else
            {
                return totalPH;
            }
        }

        public double CalculatePerHourWastageLitre(SegmentLeaksEntry leak)
        {
            var list = _db.SegmentEvents;
            var entry = list
            .Where(inlist => inlist.SegmentsId == leak.SegmentsId)
            .Last();

            double usageperpoll = (entry.FlowIn - entry.FlowOut);
            if (usageperpoll < 0)
            {
                throw new Exception("ERROR : Litres Per Hour Less Than 0");
            }
            else
            {
                return (usageperpoll);
            }
        }

        public async Task<double> CalculateTotalUsageLitres(SegmentLeaksEntry leak)
        {
           var events = await _db.SegmentEvents.Where(a => a.TimeStamp >= leak.OriginalTimeStamp || a.TimeStamp <= leak.LatestTimeStamp).ToListAsync();
           var totalUsageForPeriod = 0.0;

            if (events != null || events.Count > 0)
            {
                foreach (var item in events)
                {
                    totalUsageForPeriod += (item.FlowIn / 60);
                }

                return totalUsageForPeriod;
            }
            else
            {
                throw new Exception("ERROR : Cannot Retrieve Information For Time Period");
            }

        }

        public async Task<double> CalculateTotalWastageLitres(SegmentLeaksEntry leak)
        {
            var list = await _db.SegmentEvents.ToListAsync();
            var entry = list
            .Where(inlist => inlist.SegmentsId == leak.SegmentsId)
            .Last();

            var timebetween = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours;
            var perhour = CalculatePerHourWastageLitre(leak);
            var ltotal = (timebetween * perhour);

            if(ltotal < 0)
            {
                throw new Exception("ERROR : Total Litres Less Than 0");
            }
            else
            {
                return ltotal;
            }
        }

        private string GetSegmentStatus(int segmentId)
        {
            var list = _db.SegmentEvents;
            var entry = list
            .Where(inlist => inlist.SegmentsId == segmentId)
            .Last();
            if (entry == null)
            {
                throw new Exception("ERROR : Segment Does Not Exist");
            }
            else
            {
                return entry.EventType;
            }
        }

        //Calculates the data points of the wastage based on period
        public async Task<DataPoints<DateTime, double>[]> CalculatePeriodWastageAsync(Period timeframe)
        {
            switch (timeframe)
            {
                case Period.Daily:
                    return CalculateDailyWastage(await _db
                    .SegmentEvents.Where(a => a.EventType == "leak" && a.TimeStamp.Month == DateTime.Now.Month && a.TimeStamp.Day == DateTime.Now.Day && a.TimeStamp.Year == DateTime.Now.Year)
                    .GroupBy(b => b.TimeStamp.Hour)
                    .ToListAsync());

                case Period.Monthly:
                    return (CalculateMonthlyWastage(await _db.SegmentEvents.Where(a => a.EventType == "leak")
                    .GroupBy(b => b.TimeStamp.Month)
                    .ToListAsync()));

                case Period.Seasonally:
                    var summerList = await _db.SegmentEvents
                    .Where(a => a.EventType == "leak" && GetSeason(a.TimeStamp, true) == 1)
                    .ToListAsync();

                    var winterList = await _db.SegmentEvents
                    .Where(a => a.EventType == "leak" && GetSeason(a.TimeStamp,true) == 3)
                    .ToListAsync();

                    var autumnList = await _db.SegmentEvents
                    .Where(a => a.EventType == "leak" && GetSeason(a.TimeStamp,true) == 2)
                    .ToListAsync();

                    var springList = await _db.SegmentEvents
                    .Where(a => a.EventType == "leak" && GetSeason(a.TimeStamp,true) == 0)
                    .ToListAsync();

                    return CalculateSeasonallyWastage(summerList, winterList, autumnList, springList);
                default:
                    return null;
            }
        }

        private int GetSeason(DateTime date, bool ofSouthernHemisphere)
        {
            int hemisphereConst = (ofSouthernHemisphere ? 2 : 0);
            Func<int, int> getReturn = (northern) => {
                return (northern + hemisphereConst) % 4;
            };
            float value = (float)date.Month + date.Day / 100f;  // <month>.<day(2 digit)>
            if (value < 3.21 || value >= 12.22) return getReturn(3);    // 3: Winter
            if (value < 6.21) return getReturn(0);  // 0: Spring
            if (value < 9.23) return getReturn(1);  // 1: Summer
            return getReturn(2);    // 2: Autumn
        }

        //Calculates the data points of the wastage based on period
        public async Task<DataPoints<DateTime, double>[]> SummaryPeriodUsageAsync(Period timeframe)
        {
            switch (timeframe)
            {
                case Period.Daily:
                    return SummaryDailyUsage(await _db
                    .SegmentEvents.Where(a => a.TimeStamp.Month == DateTime.Now.Month && a.TimeStamp.Day == DateTime.Now.Day && a.TimeStamp.Year == DateTime.Now.Year)
                    .GroupBy(b => b.TimeStamp.Hour)
                    .ToListAsync());

                case Period.Monthly:
                    return (SummaryMonthlyUsage(await _db.SegmentEvents.GroupBy(b => b.TimeStamp.Month)
                    .ToListAsync()));

                case Period.Seasonally:
                    var summerList = await _db.SegmentEvents
                    .Where(a => GetSeason(a.TimeStamp, true) == 1)
                    .ToListAsync();

                    var winterList = await _db.SegmentEvents
                    .Where(a =>  GetSeason(a.TimeStamp, true) == 3)
                    .ToListAsync();

                    var autumnList = await _db.SegmentEvents
                    .Where(a =>GetSeason(a.TimeStamp, true) == 2)
                    .ToListAsync();

                    var springList = await _db.SegmentEvents
                    .Where(a => GetSeason(a.TimeStamp, true) == 0)
                    .ToListAsync();

                    return SummarySeasonallyUsage(summerList, winterList, autumnList, springList);
                default:
                    return null;
            }
        }

        public async Task<DataPoints<String,double>> SummaryPeriodCostsSeasonAsync()
        {
            var summerList = await _db.SegmentEvents
                   .Where(a => GetSeason(a.TimeStamp, true) == 1)
                   .ToListAsync();

            var winterList = await _db.SegmentEvents
            .Where(a => GetSeason(a.TimeStamp, true) == 3)
            .ToListAsync();

            var autumnList = await _db.SegmentEvents
            .Where(a => GetSeason(a.TimeStamp, true) == 2)
            .ToListAsync();

            var springList = await _db.SegmentEvents
            .Where(a => GetSeason(a.TimeStamp, true) == 0)
            .ToListAsync();

            var summary = SummarySeasonsCost(summerList, winterList, autumnList, springList);
            return summary;
        }
        //Calculates the data points of the cost based on period
        public async Task<DataPoints<DateTime, double>[]> SummaryPeriodCostsAsync(Period timeframe)
        {
            switch (timeframe)
            {
                case Period.Daily:
                    return SummaryDailyCost(await _db
                    .SegmentEvents.Where(a => a.TimeStamp.Month == DateTime.Now.Month && a.TimeStamp.Day == DateTime.Now.Day && a.TimeStamp.Year == DateTime.Now.Year)
                    .GroupBy(b => b.TimeStamp.Hour)
                    .ToListAsync());

                case Period.Monthly:
                    return (SummaryMonthlyCost(await _db.SegmentEvents.GroupBy(b => b.TimeStamp.Month)
                    .ToListAsync()));
                default:

                    return null;
            }
        }
        //Returns an array of yearly sorted data
        // 0 - summer
        // 1 - winter
        // 2 - spring
        // 3 - autumn
        public DataPoints<DateTime, double>[] CalculateSeasonallyWastage(List<SegmentEventsEntry> summer, List<SegmentEventsEntry> winter, List<SegmentEventsEntry> autumn, List<SegmentEventsEntry> spring)
        {
            try
            {
                //Get Summer
                var sortedSummer = CalculateYearlyWastage(summer.GroupBy(a => a.TimeStamp.Month).ToList());
                //Get Winter
                var sortedWinter = CalculateYearlyWastage(winter.GroupBy(a => a.TimeStamp.Month).ToList());
                //Get Autumn
                var sortedAutumn = CalculateYearlyWastage(autumn.GroupBy(a => a.TimeStamp.Month).ToList());
                //Get Spring
                var sortedSpring = CalculateYearlyWastage(spring.GroupBy(a => a.TimeStamp.Month).ToList());

                DataPoints<DateTime, double>[] arrayOfSeasons = new DataPoints<DateTime, double>[4];

                arrayOfSeasons[0] = sortedSummer[0];
                arrayOfSeasons[1] = sortedWinter[0];
                arrayOfSeasons[2] = sortedSpring[0];
                arrayOfSeasons[3] = sortedAutumn[0];

                return arrayOfSeasons;
            }
            catch(Exception error)
            {
                throw new Exception(error.Message);
            }
        }

        public DataPoints<DateTime, double>[] CalculateYearlyWastage(List<IGrouping<int, SegmentEventsEntry>> list)
        {
            try
            {
                DataPoints<DateTime, double> yearly = new DataPoints<DateTime, double>();
                var totalForMonth = 0.0;
                for (int i = 0; i < list.Count; i++)
                {
                    //We have a list per hour of current day.
                    //Group these groups by segmentId
                    totalForMonth = 0.0;
                    var segments = list.ElementAt(i).GroupBy(a => a.SegmentsId);
                    foreach (IGrouping<int, SegmentEventsEntry> lst in segments)
                    {
                        foreach (SegmentEventsEntry lst2 in lst)
                        {
                            totalForMonth += ((lst2.FlowIn - lst2.FlowOut) / 60);
                        }
                    }
                    yearly.AddPoint(list.ElementAt(i).ElementAt(0).TimeStamp, totalForMonth);
                }
                DataPoints<DateTime, double>[] ret = new DataPoints<DateTime, double>[1];

                ret[0] = yearly;
                return ret;
            }
            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        }

        public DataPoints<DateTime, double>[] CalculateMonthlyWastage(List<IGrouping<int, SegmentEventsEntry>> list)
        {
            try
            {
                DataPoints<DateTime, double> monthly = new DataPoints<DateTime, double>();
                var totalForDay = 0.0;
                for (int i = 0; i < list.Count; i++)
                {
                    //We have a list per hour of current day.
                    //Group these groups by segmentId
                    totalForDay = 0.0;
                    var segments = list.ElementAt(i).GroupBy(a => a.SegmentsId);
                    foreach (IGrouping<int, SegmentEventsEntry> lst in segments)
                    {
                        foreach (SegmentEventsEntry lst2 in lst)
                        {
                            totalForDay += ((lst2.FlowIn - lst2.FlowOut) / 60);

                        }

                    }
                    monthly.AddPoint(list.ElementAt(i).ElementAt(0).TimeStamp, totalForDay);
                }

                //See what months to add
                List<int> monthsAlreadyThere = new List<int>();
                foreach (DataNode<DateTime, double> var in monthly.dataPoints)
                {
                    monthsAlreadyThere.Add(var.x.Month);
                }

                for (int i = 1; i <= 12; i++)
                {
                    if (!(monthsAlreadyThere.Contains(i)))
                    {
                        monthly.AddPoint(new DateTime(2000, i, 1), 0.0);
                    }
                }
                monthly.dataPoints = monthly.dataPoints.OrderBy(a => a.x.Month).ToList();
                DataPoints<DateTime, double>[] ret = new DataPoints<DateTime, double>[1];
                ret[0] = monthly;
                return ret;
            }
            catch(Exception error)
            {
                throw new Exception(error.Message);
            }
        }

        public DataPoints<DateTime, double>[] CalculateDailyWastage(List<IGrouping<int, SegmentEventsEntry>> list)
        {
            try
            {
                DataPoints<DateTime, double> daily = new DataPoints<DateTime, double>();
                var totalForHour = 0.0;
                for (int i = 0; i < list.Count; i++)
                {
                    //We have a list per hour of current day.
                    //Group these groups by segmentId
                    totalForHour = 0.0;
                    var segments = list.ElementAt(i).GroupBy(a => a.SegmentsId);
                    foreach (IGrouping<int, SegmentEventsEntry> lst in segments)
                    {
                        foreach (SegmentEventsEntry lst2 in lst)
                        {
                            totalForHour += ((lst2.FlowIn - lst2.FlowOut) / 60);
                        }
                    }
                    daily.AddPoint(list.ElementAt(i).ElementAt(0).TimeStamp, totalForHour);
                }
                DataPoints<DateTime, double>[] ret = new DataPoints<DateTime, double>[1];
                ret[0] = daily;
                return ret;
            }
            catch(Exception error)
            {
                throw new Exception(error.Message);
            }
        }

        public enum Period
        {
            Daily,
            Seasonally,
            Monthly
        }

        public DataPoints<DateTime, double>[] SummaryDailyUsage(List<IGrouping<int, SegmentEventsEntry>> list)
        {
            DataPoints<DateTime, double> daily = new DataPoints<DateTime, double>();
            var totalForHour = 0.0;
            for (int i = 0; i < list.Count; i++)
            {
                //We have a list per hour of current day.
                //Group these groups by segmentId
                totalForHour = 0.0;
                var segments = list.ElementAt(i).GroupBy(a => a.SegmentsId);
                foreach (IGrouping<int, SegmentEventsEntry> lst in segments)
                {
                    foreach (SegmentEventsEntry lst2 in lst)
                    {
                        totalForHour += ((lst2.FlowIn) / 60);
                    }
                }
                daily.AddPoint(list.ElementAt(i).ElementAt(0).TimeStamp, totalForHour);
            }
            DataPoints<DateTime, double>[] ret = new DataPoints<DateTime, double>[1];
            ret[0] = daily;
            return ret;
        }

        //public double summaryMonthlyUsage(List<IGrouping<int, SegmentEventsEntry>> list)
        public DataPoints<DateTime, double>[] SummaryMonthlyUsage(List<IGrouping<int, SegmentEventsEntry>> list)
        {
            DataPoints<DateTime, double> monthly = new DataPoints<DateTime, double>();
            var totalForDay = 0.0;
            for (int i = 0; i < list.Count; i++)
            {
                //We have a list per hour of current day.
                //Group these groups by segmentId
                totalForDay = 0.0;
                var segments = list.ElementAt(i).GroupBy(a => a.SegmentsId);
                foreach (IGrouping<int, SegmentEventsEntry> lst in segments)
                {
                    foreach (SegmentEventsEntry lst2 in lst)
                    {
                        totalForDay += ((lst2.FlowIn) / 60);
                    }
                }
                monthly.AddPoint(list.ElementAt(i).ElementAt(0).TimeStamp, totalForDay);
            }

            //See what months to add
            int month = 1;
            List<int> monthsAlreadyThere = new List<int>();
            foreach (DataNode<DateTime, double> var in monthly.dataPoints)
            {
                monthsAlreadyThere.Add(var.x.Month);
            }

            for (int i = 1; i <= 12; i++)
            {
                if (!(monthsAlreadyThere.Contains(i)))
                {
                    monthly.AddPoint(new DateTime(2000, i, 1), 0.0);
                }
            }
            monthly.dataPoints = monthly.dataPoints.OrderBy(a => a.x.Month).ToList();
            DataPoints<DateTime, double>[] ret = new DataPoints<DateTime, double>[1];
            ret[0] = monthly;
            return ret;
        }

        public DataPoints<DateTime, double>[] YearlyUsage(List<IGrouping<int, SegmentEventsEntry>> list)
        {
            DataPoints<DateTime, double> yearly = new DataPoints<DateTime, double>();
            var totalForMonth = 0.0;
            for (int i = 0; i < list.Count; i++)
            {
                //We have a list per hour of current day.
                //Group these groups by segmentId
                totalForMonth = 0.0;
                var segments = list.ElementAt(i).GroupBy(a => a.SegmentsId);
                foreach (IGrouping<int, SegmentEventsEntry> lst in segments)
                {
                    foreach (SegmentEventsEntry lst2 in lst)
                    {
                        totalForMonth += ((lst2.FlowIn) / 60);
                    }
                }
                yearly.AddPoint(list.ElementAt(i).ElementAt(0).TimeStamp, totalForMonth);
            }
            DataPoints<DateTime, double>[] ret = new DataPoints<DateTime, double>[1];
               ret[0] = yearly;
            return ret;
        }

        public DataPoints<DateTime, double>[] SummarySeasonallyUsage(List<SegmentEventsEntry> summer, List<SegmentEventsEntry> winter, List<SegmentEventsEntry> autumn, List<SegmentEventsEntry> spring)
        {
            //Get Summer
            var sortedSummer = YearlyUsage(summer.GroupBy(a => a.TimeStamp.Month).ToList());
            //Get Winter
            var sortedWinter = YearlyUsage(winter.GroupBy(a => a.TimeStamp.Month).ToList());
            //Get Autumn
            var sortedAutumn = YearlyUsage(autumn.GroupBy(a => a.TimeStamp.Month).ToList());
            //Get Spring
            var sortedSpring = YearlyUsage(spring.GroupBy(a => a.TimeStamp.Month).ToList());

            DataPoints<DateTime, double>[] arrayOfSeasons = new DataPoints<DateTime, double>[4];
            arrayOfSeasons[0] = sortedSummer[0];
            arrayOfSeasons[1] = sortedWinter[0];
            arrayOfSeasons[2] = sortedSpring[0];
            arrayOfSeasons[3] = sortedAutumn[0];
            return arrayOfSeasons;
        }

        public DataPoints<DateTime, double>[] SummaryDailyCost(List<IGrouping<int, SegmentEventsEntry>> list)
        {
            DataPoints<DateTime, double> daily = new DataPoints<DateTime, double>();
            var totalForHour = 0.0;
            for (int i = 0; i < list.Count; i++)
            {
                //We have a list per hour of current day.
                //Group these groups by segmentId
                totalForHour = 0.0;
                var segments = list.ElementAt(i).GroupBy(a => a.SegmentsId);
                foreach (IGrouping<int, SegmentEventsEntry> lst in segments)
                {
                    foreach (SegmentEventsEntry lst2 in lst)
                    {
                        totalForHour += ((lst2.FlowIn) / 60);
                    }
                 }
                double cost = (totalForHour) * 37;
                daily.AddPoint(list.ElementAt(i).ElementAt(0).TimeStamp, cost);
            }
            DataPoints<DateTime, double>[] ret = new DataPoints<DateTime, double>[1];
            ret[0] = daily;
            return ret;
        }

        public DataPoints<DateTime, double>[] SummaryMonthlyCost(List<IGrouping<int, SegmentEventsEntry>> list)
        {
            DataPoints<DateTime, double> monthly = new DataPoints<DateTime, double>();
            var totalForDay = 0.0;
            for (int i = 0; i < list.Count; i++)
            {
                //We have a list per hour of current day.
                //Group these groups by segmentId
                totalForDay = 0.0;
                var segments = list.ElementAt(i).GroupBy(a => a.SegmentsId);
                foreach (IGrouping<int, SegmentEventsEntry> lst in segments)
                {
                    foreach (SegmentEventsEntry lst2 in lst)
                    {
                        totalForDay += ((lst2.FlowIn) / 60);
                    }

                }
                double cost = (totalForDay) * 37;
                monthly.AddPoint(list.ElementAt(i).ElementAt(0).TimeStamp, cost);
            }

            //See what months to add
            int month = 1;
            List<int> monthsAlreadyThere = new List<int>();
            foreach (DataNode<DateTime, double> var in monthly.dataPoints)
            {
                monthsAlreadyThere.Add(var.x.Month);
            }

            for (int i = 1; i <= 12; i++)
            {
                if (!(monthsAlreadyThere.Contains(i)))
                {
                    monthly.AddPoint(new DateTime(2000, i, 1), 0.0);
                }
            }
            monthly.dataPoints = monthly.dataPoints.OrderBy(a => a.x.Month).ToList();
            DataPoints<DateTime, double>[] ret = new DataPoints<DateTime, double>[1];
            ret[0] = monthly;
            return ret;
        }
        public DataPoints<String, double> SummarySeasonsCost(List<SegmentEventsEntry> summer, List<SegmentEventsEntry> winter, List<SegmentEventsEntry> autumn, List<SegmentEventsEntry> spring)
        {
            DataPoints<DateTime,double>[] usage = SummarySeasonallyUsage(summer, winter, autumn, spring);
            List<double> cost_season = new List<double>();

            //Summer season
            if (usage[0].dataPoints.Count != 0)
            {
                List<double> vals = usage[0].getv();
                double sum = 0;
                for (int i = 0; i < vals.Count; i++)
                {
                    sum += vals[i];
                }
                double summer_cost = sum * 37;
                cost_season.Add(summer_cost);
            }
            else
            {
                cost_season.Add(0);
            }

            //winter season
            if (usage[1].dataPoints.Count != 0)
            {
                List<double> vals1 = usage[1].getv();
                double sum1 = 0;

                for (int i = 0; i < vals1.Count; i++)
                {
                    sum1 += vals1[i];
                }
                double winter_cost = sum1 * 37;
                cost_season.Add(winter_cost);
            }
            else
            {
                cost_season.Add(0);
            }
            //spring season
            if (usage[2].dataPoints.Count != 0)
            {
                List<double> vals2 = usage[2].getv();
                double sum2 = 0;

                for (int i = 0; i < vals2.Count; i++)
                {
                    sum2 += vals2[i];
                }

                double spring_cost = sum2 * 37;
                cost_season.Add(spring_cost);
            }
            else
            {
                cost_season.Add(0);
            }
            //Autum season
            if (usage[3].dataPoints.Count != 0)
            {
                List<double> vals3 = usage[3].getv();
                double sum3 = 0;

                for (int i = 0; i < vals3.Count; i++)
                {
                    sum3 += vals3[i];
                }
                double autum_cost = sum3 * 37;
                cost_season.Add(autum_cost);
            }
            else
            {
                cost_season.Add(0);
            }
            DataPoints<String, double> arrayOfSeasonsCost = new DataPoints<String, double>();
            arrayOfSeasonsCost.AddPoint("Summer", cost_season[0]);
            arrayOfSeasonsCost.AddPoint("Winter", cost_season[1]);
            arrayOfSeasonsCost.AddPoint("Spring", cost_season[2]);
            arrayOfSeasonsCost.AddPoint("Autum", cost_season[3]);
            return arrayOfSeasonsCost;
        }
    } 
}


