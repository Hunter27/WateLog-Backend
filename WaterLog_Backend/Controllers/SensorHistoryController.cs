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
using Newtonsoft.Json;
using WaterLog_Backend.Models;

namespace WaterLog_Backend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SensorHistoryController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        private IControllerService _service;

        public SensorHistoryController(DatabaseContext context, IConfiguration config, IControllerService service)
        {
            _db = context;
            _config = config;
            _service = service;
        }

        // GET api/sensor history
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SensorHistoryEntry>>> Get()
        {
            return await _db.SensorHistory.ToListAsync();
        }

        // GET api/sensorHistory/
        [HttpGet("{id}")]
        public async Task<ActionResult<SensorHistoryEntry>> Get(int id)
        {
            return await _db.SensorHistory.FindAsync(id);
        }
        // POST api/sensorHistory
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] SensorHistoryEntry value)
        {
            if (ModelState.IsValid)
            {
                await _db.SensorHistory.AddAsync(value);
                await _db.SaveChangesAsync();
                return Ok("Ok");
            }
            else
            {
                return new BadRequestObjectResult("Not Found");
            }
        }
        // DELETE api/sensorHistory/
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            try
            {
                var entry = await _db.SensorHistory.FindAsync(id);
                _db.SensorHistory.Remove(entry);
                await _db.SaveChangesAsync();
            }
            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        } 
    }
}
