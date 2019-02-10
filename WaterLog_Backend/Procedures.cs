using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using EmailNotifications;
using WaterLog_Backend.Controllers;
using WaterLog_Backend.Models;

using WebApplication1;

namespace WaterLog_Backend
{
    public class Procedures
    {
        DatabaseContext _db;
        IConfiguration _config;
        public Procedures(DatabaseContext db, IConfiguration cfg)
        {
            _db = db;
            _config = cfg;
        }

        public async Task triggerInsert(ReadingsEntry value)
        {
            SegmentsEntry segment = await _db.Segments.Where(ins => ins.SenseIDIn == value.MonitorsId).SingleOrDefaultAsync();
            if (segment == null)
            {
              return;
            }
            //IEnumerable<SegmentsEntry> allSegments = _db.Segments;
            int segmentInid = -1;
            int segmentOutid = -1;
            int segmentid = -1;

            segmentInid = segment.SenseIDIn;
            segmentOutid = segment.SenseIDOut;
            segmentid = segment.Id;

            ReadingsEntry reading1 = await _db.Readings.Where(r => r.MonitorsId == segmentInid).OrderByDescending(r => r.TimesStamp).FirstAsync();
            ReadingsEntry reading2 = await _db.Readings.Where(re => re.MonitorsId == segmentOutid).OrderByDescending(re => re.TimesStamp).FirstAsync();

            if (isLeakage(reading1.Value, reading2.Value))
            {
                await CreateSegmentsEventAsync(segmentid, "leak", reading1.Value, reading2.Value);
                //Updateleakagestatus
                if (await _db.SegmentLeaks.AnyAsync(leak => leak.SegmentsId == segmentid))
                {
                    SegmentLeaksEntry latestEntry = await _db.SegmentLeaks.Where(leak => leak.SegmentsId == segmentid).OrderByDescending(lk => lk.LatestTimeStamp).FirstAsync();
                    //Check in SegmentEntry if latest event related to entry has been resolved.
                    if (latestEntry != null)
                    {
                        SegmentEventsEntry entry = (await _db.SegmentEvents.Where(leak => leak.SegmentsId == segmentid).OrderByDescending(lks => lks.TimeStamp).FirstAsync());
                        if (entry.EventType == "leak")
                        {
                            await updateSegmentLeaksAsync(latestEntry.Id, segmentid, calculateSeverity(segmentid), latestEntry.OriginalTimeStamp, entry.TimeStamp, "unresolved");
                        }
                    }
                }
                else
                {
                    //Normal Add
                    await createSegmentLeaksAsync(segmentid, calculateSeverity(segmentid), "unresolved");
                    string[] template = populateEmail(segmentid);
                    Email email = new Email(template, _config);
                    email.sendEmail();
                }
            }
            else
            {
                //Updatewithoutleakagestatus
                await CreateSegmentsEventAsync(segmentid, "normal", reading1.Value, reading2.Value);
            }
        }

        private string calculateSeverity(int segmentid)
        {
            return "severe";
        }

        public async Task createSegmentLeaksAsync(int segId, string severity, string resolvedStatus)
        {
            SegmentLeaksEntry entry = new SegmentLeaksEntry();
            entry.SegmentsId = segId;
            entry.Severity = severity;
            entry.LatestTimeStamp = DateTime.UtcNow;
            entry.OriginalTimeStamp = DateTime.UtcNow;
            entry.ResolvedStatus = resolvedStatus;
            await _db.SegmentLeaks.AddAsync(entry);
            await _db.SaveChangesAsync();
        }

        public async Task updateSegmentLeaksAsync(int leakId, int segId, string severity, DateTime original, DateTime updated, string resolvedStatus)
        {
            //SegmentLeaksController controller = getSegmentLeaksController();
            SegmentLeaksEntry entry = new SegmentLeaksEntry();
            entry.SegmentsId = segId;
            entry.Severity = severity;
            entry.OriginalTimeStamp = original;
            entry.LatestTimeStamp = updated;
            entry.ResolvedStatus = resolvedStatus;
            entry.Id = leakId;
            var old = await _db.SegmentLeaks.FindAsync(leakId);
            _db.Entry(old).CurrentValues.SetValues(entry);
            await _db.SaveChangesAsync();
        }

        public async Task CreateSegmentsEventAsync(int id, string status, double inv, double outv)
        {
            SegmentEventsEntry entry = new SegmentEventsEntry();
            entry.TimeStamp = DateTime.UtcNow;
            entry.SegmentsId = id;
            entry.FlowIn = inv;
            entry.FlowOut = outv;
            entry.EventType = status;
            await _db.SegmentEvents.AddAsync(entry);
            await _db.SaveChangesAsync();
        }

        public Boolean isLeakage(double first, double second)
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
            var leaks = _db.SegmentLeaks;
            var leak = leaks.Where(sudo => sudo.SegmentsId == sectionid).Single();
            string[] template = { "Segment " + leak.SegmentsId, getSegmentStatus(leak.SegmentsId), leak.Severity, getLeakPeriod(leak), calculateTotalCost(leak).ToString(), calculatePerHourCost(leak).ToString(), calculateLitresPerHour(leak).ToString(), buildUrl(leak.SegmentsId) };
            return template;
        }

        private string getLeakPeriod(SegmentLeaksEntry leak)
        {
            if (((leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours) < 1)
            {
                return "1";
            }
            else
            {
                return ((leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalDays.ToString());
            }
        }

        public double calculateTotalCost(SegmentLeaksEntry leak)
        {
            var list = _db.SegmentEvents;
            var entry = list.Where(inlist => inlist.SegmentsId == leak.SegmentsId).Last();
            var timebetween = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours;
            if (timebetween < 1)
            {
                return calculatePerHourCost(leak) / 60;
            }
            else
            {
                timebetween = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours;
            }
            var perhour = calculatePerHourCost(leak);
            return (timebetween * perhour);
        }

        private string buildUrl(int segmentId)
        {
            return "https://iot.retrotest.co.za/alert/segment/" + segmentId;
        }

        public double calculatePerHourCost(SegmentLeaksEntry leak)
        {
            var list = _db.SegmentEvents;
            var entry = list.Where(inlist => inlist.SegmentsId == leak.SegmentsId).Last();
            double currentTariff = 37.5;
            double usageperpoll = (entry.FlowIn - entry.FlowOut);
            return (usageperpoll * currentTariff);
        }

        public double calculateLitresPerHour(SegmentLeaksEntry leak)
        {
            var list = _db.SegmentEvents;
            var entry = list.Where(inlist => inlist.SegmentsId == leak.SegmentsId).Last();
            double usageperpoll = (entry.FlowIn - entry.FlowOut);
            return (usageperpoll);
        }

        public double calculateTotaLitres(SegmentLeaksEntry leak)
        {
            var list = _db.SegmentEvents;
            var entry = list.Where(inlist => inlist.SegmentsId == leak.SegmentsId).Last();
            var timebetween = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours;
            var perhour = calculateLitresPerHour(leak);
            return (timebetween * perhour);
        }

        private string getSegmentStatus(int segmentId)
        {
            var list = _db.SegmentEvents;
            var entry = list.Where(inlist => inlist.SegmentsId == segmentId).Last();
            return entry.EventType;
        }
    }
}
