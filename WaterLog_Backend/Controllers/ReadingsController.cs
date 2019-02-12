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
        public async Task<ActionResult<IEnumerable<Reading>>> Get()
        {
            return await _db.Reading.ToListAsync();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Reading>> Get(int id)
        {
            return await _db.Reading.FindAsync(id);
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] Reading value)
        {
            value.TimesStamp = DateTime.UtcNow;
            await _db.Reading.AddAsync(value);
            await _db.SaveChangesAsync();
            Procedures procedure = new Procedures(_db, _config);
            await procedure.triggerInsert(value);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] Reading value)
        {
            var old = await _db.Reading.FindAsync(id);
            _db.Entry(old).CurrentValues.SetValues(value);
            await _db.SaveChangesAsync();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var entry = await _db.Reading.FindAsync(id);
            _db.Reading.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}
