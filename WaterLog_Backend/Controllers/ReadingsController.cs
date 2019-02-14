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
                value.TimesStamp = DateTime.Now;
                await _db.Readings.AddAsync(value);
                await _db.SaveChangesAsync();
                Procedures procedure = new Procedures(_db, _config);
                await procedure.triggerInsert(value);
        }

        // PUT api/values/5
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
