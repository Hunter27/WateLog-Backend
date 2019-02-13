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
    public class PumpsController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public PumpsController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PumpEntry>>> Get()
        {
            return await _db.Pumps.ToListAsync();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<PumpEntry>> Get(int id)
        {
            return await _db.Pumps.FindAsync(id);
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] PumpEntry value)
        {
            await _db.Pumps.AddAsync(value);
            await _db.SaveChangesAsync();
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] PumpEntry value)
        {
            var old = await _db.Pumps.FindAsync(id);
            _db.Entry(old).CurrentValues.SetValues(value);
            await _db.SaveChangesAsync();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var entry = await _db.Pumps.FindAsync(id);
            _db.Pumps.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}

