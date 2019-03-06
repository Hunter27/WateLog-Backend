using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Cors;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using WaterLog_Backend.Models;

namespace WaterLog_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorMonitorController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;

        public SensorMonitorController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // GET api/Sensormonitor
        [HttpGet("{id}/{date}")]
        public async Task<ActionResult> Get(int id, DateTime date)
        {
            return Ok("date: " + date + "id: " + id);
        }

       // GET api/Sensormonitor
       [HttpPut("{id}/{date}")]
        public async Task<ActionResult> Put(int id, DateTime date, [FromBody] MonitorsEntry Monitor_value)
        { 
            var old_monitor = await _db.Monitors.FindAsync(id); 
            _db.Entry(old_monitor).CurrentValues.SetValues(Monitor_value);
            var oldhistories = await _db.SensorHistory.Where(history => history.SensorId == id).ToListAsync();
            var oldhistory = oldhistories.Where(history => history.FaultDate == date).FirstOrDefault();
            SensorHistoryEntry newHistory = oldhistory; 
            newHistory.SensorResolved = oldhistory.SensorResolved == EnumResolveStatus.UNRESOLVED 
                ? EnumResolveStatus.RESOLVED 
                : EnumResolveStatus.UNRESOLVED;
            _db.Entry(oldhistory).CurrentValues.SetValues(newHistory);
            await _db.SaveChangesAsync();
            return Ok(Monitor_value);
        }  
    }
}