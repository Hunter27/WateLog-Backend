using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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
    public class MonitorsController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public MonitorsController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MonitorsEntry>>> Get()
        {
            
            return await _db.Monitors.ToListAsync();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<MonitorsEntry>> Get(int id)
        {
            return await _db.Monitors.FindAsync(id);
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] MonitorsEntry value)
        {
            await _db.Monitors.AddAsync(value);
            await _db.SaveChangesAsync();
            
            ;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] MonitorsEntry value)
        {
            var entry = await _db.Monitors.FindAsync(id);
            entry = value;
            await _db.SaveChangesAsync();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var entry = await _db.Monitors.FindAsync(id);
            _db.Monitors.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}