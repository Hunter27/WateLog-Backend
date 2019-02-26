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
        enum FaultHeat : int { High = 5, Medium = 3, Low = 1 };
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
                await CreateSegmentsEventAsync(segmentid, "leak", reading1.Value, reading2.Value);
                var leakExists = await _db.SegmentLeaks
                    .Where(leak => leak.SegmentsId == segmentid && leak.ResolvedStatus == EnumResolveStatus.UNRESOLVED)
                    .OrderByDescending(lk => lk.LatestTimeStamp)
                    .FirstAsync();
                //Updateleakagestatus
                if (leakExists != null)
                {
                    //Check in SegmentEntry if latest event related to entry has been resolved
                        SegmentEventsEntry entry = await _db.SegmentEvents
                        .Where(leak => leak.SegmentsId == segmentid)
                        .OrderByDescending(lks => lks.TimeStamp)
                        .FirstAsync();

                        if (entry.EventType == "leak")
                        {
                            var severity = await CalculateSeverity(leakExists);
                            await UpdateSegmentLeaksAsync(leakExists.Id, segmentid,severity,leakExists.OriginalTimeStamp, 
                            entry.TimeStamp,EnumResolveStatus.UNRESOLVED,leakExists.LastNotificationDate);
                        }
                }
                else
                {
                    //Normal Add
                    await CreateSegmentLeaksAsync(segmentid, EnumResolveStatus.UNRESOLVED);
                    //Call an initial email
                    //Get recipients.
                    var mailing = await _db.MailingList.Where(a => a.ListGroup == "tier2").ToListAsync();
                    if (mailing.Count > 0)
                    {
                        var lastInsert = await _db.SegmentLeaks.LastAsync();
                        if (lastInsert != null)
                        {
                            string[] template = await populateEmailAsync(lastInsert);
                            Email email = new Email(template, _config);
                            Recipient[] mailers = new Recipient[(mailing.Count - 1)];
                            int countForMailers = 0;
                            foreach (var rec in mailing)
                            {
                                mailers[countForMailers] = new Recipient(rec.Address, (rec.Name + " " + rec.Surname));
                            }
                            email.SendMail(mailers);
                        }
                    }
                }
            }
            else
            {
                //Updatewithoutleakagestatus
                await UpdateSegmentsEventAsync(segmentid, "normal", reading1.Value, reading2.Value);
            }
        }

        private async Task CreateSegmentsEventAsync(int id, string status, double inv, double outv)
        {
            SegmentEventsEntry entry = new SegmentEventsEntry();
            entry.TimeStamp = DateTime.Now;
            entry.SegmentsId = id;
            entry.FlowIn = inv;
            entry.FlowOut = outv;
            entry.EventType = status;
            await _db.SegmentEvents.AddAsync(entry);
            await _db.SaveChangesAsync();
        }

        public async Task<string> CalculateSeverity(SegmentLeaksEntry entry)
        {
            var wastageLitres = await CalculateTotalWastageLitres(entry);
            return CalculateSeverityGivenValue(wastageLitres);

            
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

        public async Task CreateSegmentLeaksAsync(int segId, EnumResolveStatus resolvedStatus)
        {
            SegmentLeaksEntry entry = new SegmentLeaksEntry();
            entry.SegmentsId = segId;
            entry.LatestTimeStamp = DateTime.Now;
            entry.OriginalTimeStamp = DateTime.Now;
            entry.LastNotificationDate = DateTime.Now;
            entry.ResolvedStatus = resolvedStatus;
            entry.Severity = await CalculateSeverity(entry);
            await _db.SegmentLeaks.AddAsync(entry);
            await _db.SaveChangesAsync();

        }
        
        public async Task UpdateSegmentLeaksAsync(int leakId, int segId, string severity, DateTime original, DateTime updated, EnumResolveStatus resolvedStatus,DateTime lastEmail)
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
                    string[] template = await populateEmailAsync(entry);
                    Email email = new Email(template, _config);
                    if ((DateTime.Now - lastEmail).Days >= 4)
                    {
                        //Send to tier 1
                        var tier1 = await GetTier1ListAsync();
                        email.SendMail(tier1);
                    }
                    else
                    {
                        //Send to tier 2
                        var tier2 = await GetTier2ListAsync();
                        email.SendMail(tier2);
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
            else if (mailing.Count == 1)
            {
                Recipient[] mailers = new Recipient[1];

                mailers[0] = new Recipient(
                    mailing.ElementAtOrDefault(0).Address,
                    mailing.ElementAtOrDefault(0).Name + " " + mailing.ElementAtOrDefault(0).Surname
                );

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
            if (mailing.Count > 1)
            {
                Recipient[] mailers = new Recipient[(mailing.Count - 1)];
                int countForMailers = 0;
                foreach (var rec in mailing)
                {
                    mailers[countForMailers] = new Recipient(rec.Address, (rec.Name + " " + rec.Surname));
                }
                return mailers;
            }
            else if(mailing.Count == 1)
            {
                Recipient[] mailers = new Recipient[1];

                mailers[0] = new Recipient(
                    mailing.ElementAtOrDefault(0).Address,
                    mailing.ElementAtOrDefault(0).Name + " " + mailing.ElementAtOrDefault(0).Surname
                );

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
            double margin = 4;
            if ((first - second) > margin)
            {
                return true;
            }
            return false;
        }

        public async Task<string[]> populateEmailAsync(SegmentLeaksEntry section)
        {
            try
            {
                var totalCost = await CalculateTotalCostAsync(section);
                var perHourWastageCost = await CalculatePerHourWastageCost(section);
                var perHourWastageLitre = await CalculatePerHourWastageLitre(section);
                string[] template = 
                {
                 "Segment " + section.SegmentsId,
                  GetSegmentStatus(section.SegmentsId),
                  section.Severity,
                  GetLeakPeriod(section),
                  Math.Round(totalCost).ToString(),
                  Math.Round(perHourWastageCost).ToString(),
                  Math.Round(perHourWastageLitre).ToString(),
                  BuildUrl(section.SegmentsId)
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

        public async Task<double> CalculateTotalCostAsync(SegmentLeaksEntry leak)
        {
            var list = _db.SegmentEvents;
            var entry = list
            .Where(inlist => inlist.SegmentsId == leak.SegmentsId)
            .Last();

            var timebetween = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours;
            if (timebetween < 1)
            {
                return await CalculatePerHourWastageCost(leak)/60;
            }
            else
            {
                timebetween = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours;
            }
            var perhour = await CalculatePerHourWastageCost(leak);
            var total =  (timebetween * perhour);

            return Math.Max(total, 0);
        }

        private string BuildUrl(int segmentId)
        {
            return "https://iot.retrotest.co.za/alert/segment/" + segmentId;
        }

        public async Task<double> CalculatePerHourWastageLitre(SegmentLeaksEntry leak)
        {
            var list = await _db.SegmentEvents.Where(inlist => inlist.SegmentsId == leak.SegmentsId && 
                inlist.EventType == "leak" && inlist.TimeStamp >= leak.OriginalTimeStamp && 
                inlist.TimeStamp <= leak.LatestTimeStamp).ToListAsync();

            double usageperpoll = 0.0;
            foreach (var item in list)
            {
                usageperpoll += (item.FlowIn - item.FlowOut);
            }
            usageperpoll *= 0.0167;
            var minutes = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalMinutes;
            usageperpoll /= minutes;
            usageperpoll *= Globals.MinuteToHour;
            return Math.Max(usageperpoll,0);
        }

        public async Task<double> CalculatePerHourWastageCost(SegmentLeaksEntry leak)
        {
            var list = await _db.SegmentEvents.Where(inlist => inlist.SegmentsId == leak.SegmentsId &&
                inlist.EventType == "leak" && inlist.TimeStamp >= leak.OriginalTimeStamp && 
                inlist.TimeStamp <= leak.LatestTimeStamp).ToListAsync();

            double usageperpoll = 0.0;
            foreach(var item in list)
            {
                usageperpoll += (item.FlowIn - item.FlowOut);
            }
            //Convert Poll Into Minutes
            usageperpoll *= 0.0167;
            var minutes = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalMinutes;
            usageperpoll /= minutes;
            usageperpoll *= Globals.MinuteToHour;
            return Math.Max(((usageperpoll) * Globals.RandPerLitre),0);

        }

        public async Task<double> CalculateTotalUsageLitres(SegmentLeaksEntry leak)
        {
           var events = await _db.SegmentEvents.Where(a => a.TimeStamp >= leak.OriginalTimeStamp || a.TimeStamp <= leak.LatestTimeStamp)
                .ToListAsync();

           var totalUsageForPeriod = 0.0;

            if (events != null || events.Count > 0)
            {
                foreach (var item in events)
                {
                    totalUsageForPeriod += (item.FlowIn);
                }

                //Perhour
                var perhour = (totalUsageForPeriod *0.0167);
                var minutes = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalMinutes;
                perhour /= minutes;
                perhour *= Globals.MinuteToHour;

                //
                var timebetween = minutes / Globals.MinuteToHour;
                return (Math.Max((perhour * timebetween),0));
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

            var timebetween = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalMinutes;
            timebetween /= Globals.MinuteToHour;
            var perhour = await CalculatePerHourWastageLitre(leak);
            var ltotal = (timebetween * perhour);
            return Math.Max(ltotal, 0);
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
                    var debug = await _db
                    .SegmentEvents.Where(
                        a => a.EventType == "leak" && a.TimeStamp.Month == DateTime.Now.Month && 
                        a.TimeStamp.Day == DateTime.Now.Day && a.TimeStamp.Year == DateTime.Now.Year
                     )
                    .GroupBy(b => b.TimeStamp.Hour)
                    .ToListAsync();
                    var daily = await _db
                    .SegmentEvents.Where(
                        a => a.EventType == "leak" && a.TimeStamp.Month == DateTime.Now.Month &&
                        a.TimeStamp.Day == DateTime.Now.Day && a.TimeStamp.Year == DateTime.Now.Year
                     )
                    .GroupBy(b => b.TimeStamp.Hour)
                    .ToListAsync();
                    return CalculateDailyWastage(daily);

                case Period.Monthly:
                    var monthly = await _db.SegmentEvents.Where(a => a.EventType == "leak")
                    .GroupBy(b => b.TimeStamp.Month)
                    .ToListAsync();
                    return CalculateMonthlyWastage(monthly);

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
                    var daily = await _db
                    .SegmentEvents.Where(a => a.TimeStamp.Month == DateTime.Now.Month && a.TimeStamp.Day == DateTime.Now.Day && a.TimeStamp.Year == DateTime.Now.Year)
                    .GroupBy(b => b.TimeStamp.Hour)
                    .ToListAsync();
                    return SummaryDailyUsage(daily);

                case Period.Monthly:
                    var monthly = await _db.SegmentEvents.GroupBy(b => b.TimeStamp.Month)
                    .ToListAsync();
                    return SummaryMonthlyUsage(monthly);

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
                    var daily = await _db
                    .SegmentEvents.Where(a => a.TimeStamp.Month == DateTime.Now.Month && a.TimeStamp.Day == DateTime.Now.Day && a.TimeStamp.Year == DateTime.Now.Year)
                    .GroupBy(b => b.TimeStamp.Hour)
                    .ToListAsync();
                    return SummaryDailyCost(daily);

                case Period.Monthly:
                    var monthly = await _db.SegmentEvents.GroupBy(b => b.TimeStamp.Month)
                    .ToListAsync();
                    return SummaryMonthlyCost(monthly);
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
                double cost = (totalForHour) * Globals.RandPerLitre;
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
                double cost = (totalForDay) * Globals.RandPerLitre;
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
                double summer_cost = sum * Globals.RandPerLitre;
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
                double winter_cost = sum1 * Globals.RandPerLitre;
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

                double spring_cost = sum2 * Globals.RandPerLitre;
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
                double autum_cost = sum3 * Globals.RandPerLitre;
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

        public List<MonitorHeat> getMonitorsFaultLevels()
        {
            List<MonitorHeat> HeatValues = new List<MonitorHeat>();
            IEnumerable<MonitorsEntry> all = _db.Monitors.ToArray();
            foreach (MonitorsEntry entry in all)
            {
                MonitorHeat heat = new MonitorHeat();
                heat.Long = entry.Long;
                heat.Lat = entry.Lat;
                heat.Id = entry.Id;
                string level = "";
                if (entry.FaultCount >= 5)
                {
                    level = "High";
                }else if(entry.FaultCount >= 3)
                {
                    level = "Medium";
                }
                else if (entry.FaultCount > 0)
                {
                    level = "Low";
                }
                else
                {
                    level = "Clear";
                }
                heat.FaultLevel = level;
                HeatValues.Add(heat);
            }

            return HeatValues; 
        }

        public async Task<DataPoints<DateTime, double>> getTankGraph(int tankId)
        {
            var dailyTank = await _db
                            .TankReadings.Where(a => a.TimeStamp.Month == DateTime.Now.Month && a.TimeStamp.Year == DateTime.Now.Year && a.TankMonitorsId==tankId)
                            .GroupBy(b => b.TimeStamp.Day)
                            .ToListAsync();

            return getDailyValues(dailyTank);

        }

        public DataPoints<DateTime, double> getDailyValues(List<IGrouping<int, TankReadingsEntry>> list)
        {
            DataPoints<DateTime, double> daily = new DataPoints<DateTime, double>();
            var levelForDay = 0.0;
            DateTime time;
            for (int i = 0; i < list.Count; i++)
            {
                var element = list.ElementAt(i).OrderByDescending(a=> a.TimeStamp);
                levelForDay = element.ElementAt(0).PercentageLevel;
                time = element.ElementAt(0).TimeStamp;
                daily.AddPoint(time,levelForDay);
            }

            return daily;

        }
    } 
}


