using System;
using System.Collections.Generic;
using System.Globalization;
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
    public class ReadingsController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        private IControllerService _service;

        public ReadingsController(DatabaseContext context, IConfiguration config, IControllerService service)
        {
            _db = context;
            _config = config;
            _service = service;
        }

        // GET api/readings
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadingsEntry>>> Get()
        {
            return await _db.Readings.ToListAsync();
        }

        // GET api/readingsById/
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadingsEntry>> Get(int id)
        {
            return await _db.Readings.FindAsync(id);
        }

        // POST api/readings
        [HttpPost]
        public async Task Post([FromBody] ReadingsEntry value)
        {
            value.TimesStamp = DateTime.Now;
            await _db.Readings.AddAsync(value);
            await _db.SaveChangesAsync();
            Procedures procedure = new Procedures(_db, _config);
            await procedure.triggerInsert(value);
        
        }

        [HttpPost("{value}")]
        public async Task Post([FromBody] InputSensor values)
        {
            ReadingsEntry reading = new ReadingsEntry();
            ReadingsEntry reading2 = new ReadingsEntry();
            reading.TimesStamp = DateTime.UtcNow;
            reading2.TimesStamp = DateTime.UtcNow;
            reading.Value = values.valueIn;
            reading2.Value = values.valueOut;
            reading.MonitorsId = values.IdIn;
            reading2.MonitorsId = values.IdOut;
            await _db.Readings.AddAsync(reading);
            await _db.SaveChangesAsync();
            await _db.Readings.AddAsync(reading2);
            await _db.SaveChangesAsync();
        }

        // PUT api/readings/
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] ReadingsEntry value)
        {
            try
            {
                var old = await _db.Readings.FindAsync(id);
            _db.Entry(old).CurrentValues.SetValues(value);
            await _db.SaveChangesAsync();
            }
            catch (Exception e)
            {
                Console.WriteLine("error", e);
            }
        }

        // DELETE api/readingsById/
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var entry = await _db.Readings.FindAsync(id);
            _db.Readings.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}
