using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterLog_Backend.Controllers;
using WaterLog_Backend.Models;

namespace WaterLog_Backend
{
    public class ControllerService : IControllerService
    {
        private MonitorsController monitorController;
        private ReadingsController readingsController;
        private SegmentEventsController segmenteventsController;
        private SegmentLeaksController segmentleaksController;
        private SegmentsController segmentsController;

        public ControllerService(DatabaseContext context, IConfiguration config)
        {
           monitorController = new MonitorsController(context,config);
           readingsController = new ReadingsController(context, config,this);
           segmenteventsController = new SegmentEventsController(context,config);
           segmentleaksController = new  SegmentLeaksController(context,config,this);
           segmentsController = new SegmentsController(context,config);
        }
        public MonitorsController GetMonitorsController()
        {
            return monitorController;
        }

        public ReadingsController GetReadingsController()
        {
            return readingsController;
        }

        public SegmentEventsController GetSegmentEventsController()
        {
            return segmenteventsController;
        }

        public SegmentLeaksController GetSegmentLeaksController()
        {
            return segmentleaksController;
        }

        public SegmentsController GetSegmentsController()
        {
            return segmentsController;
        }
    }

    public interface IControllerService
    {
        MonitorsController GetMonitorsController();
        ReadingsController GetReadingsController();
        SegmentEventsController GetSegmentEventsController();
        SegmentLeaksController GetSegmentLeaksController();
        SegmentsController GetSegmentsController();
    }
}
