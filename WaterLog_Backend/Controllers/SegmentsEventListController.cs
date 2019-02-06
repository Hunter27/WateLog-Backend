using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WaterLog_Backend.Models;

namespace WaterLog_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SegmentsEventListController
    {

        private readonly DatabaseContext _db;
        SegmentEventsAccess segment;
        readonly IConfiguration _config;

        public SegmentsEventListController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
            segment = new SegmentEventsAccess(_db);
        }

        //Get SegmentEventsEntry
        [HttpGet]
        public IEnumerable<EventsList> Index()
        {
            return segment.ListEvents();
        }
    }
}
