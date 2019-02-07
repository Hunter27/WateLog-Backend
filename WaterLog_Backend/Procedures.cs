using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterLog_Backend.Controllers;
using WaterLog_Backend.Models;
using WebApplication1;

namespace WaterLog_Backend
{
    public class Procedures
    {
        IControllerService _service;
        public Procedures(IControllerService service)
        {
            _service = service;
        }

        public SegmentEventsController getSegmentsEventsController()
        {
            return _service.GetSegmentEventsController();
           
        }


        public MonitorsController getMonitorsController()
        {
            return _service.GetMonitorsController();
           

        }

        public SegmentLeaksController getSegmentLeaksController()
        {
            return _service.GetSegmentLeaksController();
           

        }

        public SegmentsController getSegmentsController()
        {
            return _service.GetSegmentsController();

        }


        public async Task triggerInsert(ReadingsEntry value)
        {
            
            SegmentsController segController = getSegmentsController();
            IEnumerable<SegmentsEntry> allSegments = segController.Get().Result.Value;
            int segmentInid = -1;
            int segmentOutid = -1;
            int segmentid = -1;
            foreach (SegmentsEntry seg in allSegments)
            {
                if (seg.SenseIDIn == value.SenseID)
                {
                    segmentInid = seg.SenseIDIn;
                    segmentOutid = seg.SenseIDOut;
                    segmentid = seg.Id;
                    break;
                }

            }


            IEnumerable<ReadingsEntry> allReadings = getReadingsController().Get().Result.Value;
            allReadings = allReadings.OrderByDescending(read => read.TimesStamp);
            ReadingsEntry r1 = null, r2 = null;
            Boolean found1 = false, found2 = false;
            foreach (ReadingsEntry read in allReadings)
            {
                if (found1 && found2)
                {
                    break;
                }
                if (read.SenseID == segmentInid && found1 == false)
                {
                    r1 = read;
                    found1 = true;
                }
                if (read.SenseID == segmentOutid && found2 == false)
                {
                    r2 = read;
                    found2 = true;

                }

            }

            if (isLeakage(r1.Value, r2.Value)) {
                //Updateleakagestatus
                await updateSegmentsEventAsync(segmentid, "leak", r1.Value, r2.Value);
                // Update SegmentLeaks
                SegmentLeaksController segmentLeaks = getSegmentLeaksController();

                
                IEnumerable<SegmentLeaksEntry> allLeaks = segmentLeaks.Get().Result.Value;
                if (allLeaks.Any(leak => leak.SegmentId == segmentid))
                {
                    //At least an instance of the leak exists
                    SegmentLeaksEntry latestEntry = allLeaks.Where(leak => leak.SegmentId == segmentid).Last();

                    //Check in SegmentEntry if latest event related to entry has been resolved.
                    if(latestEntry != null)
                    {
                        SegmentEventsController controller = getSegmentsEventsController();
                        IEnumerable<SegmentEventsEntry> allEvents = controller.Get().Result.Value;
                        SegmentEventsEntry entry = allEvents.Where(leak => leak.SegmentId == segmentid).Last();
                        if(entry.EventType == "leak")
                        {
                            //Latest event related to segment is still leaking.
                            await updateSegmentLeaksAsync(latestEntry.Id,segmentid,calculateSeverity(segmentid),latestEntry.OriginalTimeStamp,entry.TimeStamp,"unresolved");
                        }
                        
                    }
                }
                else
                {
                    //Normal Add
                    await createSegmentLeaksAsync(segmentid,calculateSeverity(segmentid),"unresolved");
                }
            }
            else
            {
                //Updatewithoutleakagestatus
                await updateSegmentsEventAsync(segmentid, "normal", r1.Value, r2.Value);
            }

        }

        private ReadingsController getReadingsController()
        {
            return _service.GetReadingsController();
        }

        private string calculateSeverity(int segmentid)
        {
            return "normal";
        }

        public async Task createSegmentLeaksAsync(int segId,string severity,string resolvedStatus)
        {
            SegmentLeaksController controller = getSegmentLeaksController();
            SegmentLeaksEntry entry = new SegmentLeaksEntry();
            entry.SegmentId = segId;
            entry.Severity = severity;
            entry.LatestTimeStamp = DateTime.UtcNow;
            entry.OriginalTimeStamp = DateTime.UtcNow;
            entry.ResolvedStatus = resolvedStatus;
            await controller.Post(entry);
        }

        public async Task updateSegmentLeaksAsync(int leakId,int segId, string severity, DateTime original,DateTime updated, string resolvedStatus)
        {
            SegmentLeaksController controller = getSegmentLeaksController();
            SegmentLeaksEntry entry = new SegmentLeaksEntry();
            entry.SegmentId = segId;
            entry.Severity = severity;
            entry.OriginalTimeStamp = original;
            entry.LatestTimeStamp = updated;
            entry.ResolvedStatus = resolvedStatus;
            entry.Id = leakId;
            await controller.Patch(entry);
            
        }

        public async Task updateSegmentsEventAsync(int id, string status,double inv,double outv)
        {
            SegmentEventsController controller = getSegmentsEventsController();
            SegmentEventsEntry entry = new SegmentEventsEntry();
            entry.TimeStamp = DateTime.UtcNow;
            entry.SegmentId = id;
            entry.FlowIn = inv;
            entry.FlowOut = outv;
            entry.EventType = status;
            await controller.Post(entry);

        }

    
        public Boolean isLeakage(double first,double second)
        {
            double margin = 2;
            if((first-second) > margin)
            {
                return true;
            }
            return false;
        }

        public EmailTemplate populateEmail(int sectionid)
        {
            SegmentLeaksController controller = getSegmentLeaksController();
            var leaks = controller.Get().Result.Value;
            var leak = leaks.Where(sudo => sudo.SegmentId == sectionid).Single();

            EmailTemplate template = new EmailTemplate("Segment" + leak.SegmentId, getSegmentStatus(leak.SegmentId), leak.Severity,getLeakPeriod(leak), calculateTotalCost(leak), calculatePerHourCost(leak), buildUrl(leak.SegmentId));
            return template;
        }

        private string getLeakPeriod(SegmentLeaksEntry leak)
        {
            return ((leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalDays.ToString());
        }

        public double calculateTotalCost(SegmentLeaksEntry leak)
        {
            SegmentEventsController controller = getSegmentsEventsController();
            var list = controller.Get().Result.Value;
            var entry = list.Where(inlist => inlist.SegmentId == leak.SegmentId).Last();

            var timebetween = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours;
            var perhour = calculatePerHourCost(leak);
            return (timebetween * perhour);
        }

        private string buildUrl(int segmentId)
        {
            return "https://localhost:3000/segment/" + segmentId;
        }

        public double calculatePerHourCost(SegmentLeaksEntry leak)
        {
            SegmentEventsController controller = getSegmentsEventsController();
            var list = controller.Get().Result.Value;
            var entry = list.Where(inlist => inlist.SegmentId == leak.SegmentId).Last();

            double currentTariff = 37.5;

            double usageperpoll = (entry.FlowIn - entry.FlowOut);

            return (usageperpoll * currentTariff);
        }

        public double calculateLitresPerHour(SegmentLeaksEntry leak)
        {
            SegmentEventsController controller = getSegmentsEventsController();
            var list = controller.Get().Result.Value;
            var entry = list.Where(inlist => inlist.SegmentId == leak.SegmentId).Last();
            double usageperpoll = (entry.FlowIn - entry.FlowOut);

           
            return (usageperpoll);
        }

        public double calculateTotaLitres(SegmentLeaksEntry leak)
        {
            SegmentEventsController controller = getSegmentsEventsController();
            var list = controller.Get().Result.Value;
            var entry = list.Where(inlist => inlist.SegmentId == leak.SegmentId).Last();

            var timebetween = (leak.LatestTimeStamp - leak.OriginalTimeStamp).TotalHours;
            var perhour = calculateLitresPerHour(leak);
            return (timebetween * perhour);
        }

        private string getSegmentStatus(int segmentId)
        {
            SegmentEventsController controller = getSegmentsEventsController();
            var list = controller.Get().Result.Value;
            var entry = list.Where(inlist => inlist.SegmentId == segmentId).Last();

            return entry.EventType;


        }
    }
}
