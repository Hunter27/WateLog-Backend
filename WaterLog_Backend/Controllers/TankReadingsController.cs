using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Cors;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using WaterLog_Backend.Models;

namespace WaterLog_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TankReadingsController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public TankReadingsController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // GET api/levels
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TankReadingsEntry>>> GetTankReadingsValues()
        {
            return await _db.TankReadings.ToListAsync();
        }

        // GET api/levelsById/
        [HttpGet("{id}")]
        public async Task<ActionResult<TankReadingsEntry>> GetTankReadingsId(int id)
        {
            return await _db.TankReadings.FindAsync(id);
        }

        [HttpGet("id/{id}")]
        public async Task<ActionResult<TankMonitorsEntry>> GetLevel(int id)
        {
            return await _db.TankMonitors.FindAsync(id);
        }

        [HttpGet("graph/{id}")]
        public async Task<ActionResult<DataPoints<DateTime, double>>> GetGraph(int id)
        {
            Procedures p = new Procedures(_db, _config);
            return await p.getTankGraph(id);
        }
        
        // POST api/level
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] TankReadingsEntry value)
        {
            if (ModelState.IsValid)
            {
                await _db.TankReadings.AddAsync(value);
                await _db.SaveChangesAsync();
                return Ok("Ok");
            }
            else{
                return new BadRequestObjectResult("Not Found");
            }
        }

        // PUT api/levelById/
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] TankReadingsEntry value)
        {
            try {
                var old = await _db.TankReadings.FindAsync(id);
                _db.Entry(old).CurrentValues.SetValues(value);
                await _db.SaveChangesAsync();
            }
            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        }
      // DELETE api/levelById/
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            try
            {
                var entry = await _db.TankReadings.FindAsync(id);
                _db.TankReadings.Remove(entry);
                await _db.SaveChangesAsync();
            }
            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        }
    }
}
