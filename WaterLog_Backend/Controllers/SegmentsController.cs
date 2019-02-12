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
    public class SegmentsController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public SegmentsController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Segment>>> Get()
        {         
            return await _db.Segment.ToListAsync();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Segment>> Get(int id)
        {
            return await _db.Segment.FindAsync(id);
        }
        
        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] Segment value)
        {
            await _db.Segment.AddAsync(value);
            await _db.SaveChangesAsync();
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] Segment value)
        {
            var old = await _db.Segments.FindAsync(id);
            _db.Entry(old).CurrentValues.SetValues(value);
            await _db.SaveChangesAsync();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var entry = await _db.Segment.FindAsync(id);
            _db.Segment.Remove(entry);
            await _db.SaveChangesAsync();
        }
    }
}
