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
        public ReadingsController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ReadingsEntry>>> Get()
        {

            return await _db.Readings.ToListAsync();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ReadingsEntry>> Get(int id)
        {
            return await _db.Readings.FindAsync(id);
        }
      
        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] ReadingsEntry value)
        {
           
            value.TimesStamp = DateTime.UtcNow;
            await _db.Readings.AddAsync(value);
            await _db.SaveChangesAsync();

            //Perform changes to SegmentEvents Table


            
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] ReadingsEntry value)
        {
            var entry = await _db.Readings.FindAsync(id);
            entry = value;
            await _db.SaveChangesAsync();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var entry = await _db.Readings.FindAsync(id);
            _db.Readings.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}