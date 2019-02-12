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

        public async Task triggerInsert(Reading value)
        {
            Segment segment = await _db.Segment.Where(ins => ins.Monitor1Id == value.MonitorId).SingleOrDefaultAsync();
            if (segment == null)
            {
                return;
            }

            int segmentInid = -1;
            int segmentOutid = -1;
            int segmentid = -1;

            segmentInid = segment.Monitor1Id;
            segmentOutid = segment.Monitor2Id;
            segmentid = segment.Id;

            Reading reading1 = await _db.Reading.Where(r => r.MonitorId == segmentInid).OrderByDescending(r => r.TimesStamp).FirstAsync();
            Reading reading2 = await _db.Reading.Where(re => re.MonitorId == segmentOutid).OrderByDescending(re => re.TimesStamp).FirstAsync();

            if (isLeakage(reading1.Value, reading2.Value))
            {
                await CreateSegmentsEventAsync(segmentid, "leak", reading1.Value, reading2.Value);
                //Updateleakagestatus
                if (await _db.ActionableEvent.AnyAsync(leak => leak.SegmentId == segmentid))
                {
                    ActionableEvent latestEntry = await _db.ActionableEvent.Where(leak => leak.SegmentId == segmentid).OrderByDescending(lk => lk.LatestTimeStamp).FirstAsync();
                    //Check in SegmentEntry if latest event related to entry has been resolved.
                    if (latestEntry != null)
                    {
                        SegmentEvent entry = (await _db.SegmentEvent.Where(leak => leak.SegmentId == segmentid).OrderByDescending(lks => lks.TimeStamp).FirstAsync());
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
            ActionableEvent entry = new ActionableEvent();
            entry.SegmentId = segId;
            entry.Severity = severity;
            entry.LatestTimeStamp = DateTime.UtcNow;
            entry.OriginalTimeStamp = DateTime.UtcNow;
            entry.Status = resolvedStatus;
            await _db.ActionableEvent.AddAsync(entry);
            await _db.SaveChangesAsync();
        }

        public async Task updateSegmentLeaksAsync(int leakId, int segId, string severity, DateTime original, DateTime updated, string resolvedStatus)
        {
            ActionableEvent entry = new ActionableEvent();
            entry.SegmentId = segId;
            entry.Severity = severity;
            entry.OriginalTimeStamp = original;
            entry.LatestTimeStamp = updated;
            entry.Status = resolvedStatus;
            entry.Id = leakId;
            var old = await _db.ActionableEvent.FindAsync(leakId);
            _db.Entry(old).CurrentValues.SetValues(entry);
            await _db.SaveChangesAsync();
        }

        public async Task CreateSegmentsEventAsync(int id, string status, double inv, double outv)
        {
            SegmentEvent entry = new SegmentEvent();
            entry.TimeStamp = DateTime.UtcNow;
            entry.SegmentId = id;
            entry.FlowIn = inv;
            entry.FlowOut = outv;
            entry.EventType = status;
            await _db.SegmentEvent.AddAsync(entry);
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
            var leaks = _db.ActionableEvent;
            var leak = leaks.Where(sudo => sudo.SegmentId == sectionid).Single();
            string[] template = { "Segment " + leak.SegmentId, getSegmentStatus(leak.SegmentId), leak.Severity, getLeakPeriod(leak), calculateTotalCost(leak).ToString(), calculatePerHourCost(leak).ToString(), calculateLitresPerHour(leak).ToString(), buildUrl(leak.SegmentId) };
            return template;
        }

        private string getLeakPeriod(ActionableEvent leak)
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

        public double calculateTotalCost(ActionableEvent leak)
        {
            var list = _db.SegmentEvent;
            var entry = list.Where(inlist => inlist.SegmentId == leak.SegmentId).Last();
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

        public double calculatePerHourCost(ActionableEvent leak)
        {
            var list = _db.SegmentEvent;
            var entry = list.Where(inlist => inlist.SegmentId == leak.SegmentId).Last();
            double currentTariff = 37.5;
            double usageperpoll = (entry.FlowIn - entry.FlowOut);
            return (usageperpoll * currentTariff);
        }

        public double calculateLitresPerHour(ActionableEvent leak)
        {
            var list = _db.SegmentEvent;
            var entry = list.Where(inlist => inlist.SegmentId == leak.SegmentId).Last();
            double usageperpoll = (entry.FlowIn - entry.FlowOut);
            return (usageperpoll);
        }

        public double calculateTotaLitres(ActionableEvent leak)
        {
            var list = _db.SegmentEvent;
            var entry = list.Where(inlist => inlist.SegmentId == leak.SegmentId).Last();
            var timebetween = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours;
            var perhour = calculateLitresPerHour(leak);
            return (timebetween * perhour);
        }

        private string getSegmentStatus(int segmentId)
        {
            var list = _db.SegmentEvent;
            var entry = list.Where(inlist => inlist.SegmentId == segmentId).Last();
            return entry.EventType;
        }
    }
}