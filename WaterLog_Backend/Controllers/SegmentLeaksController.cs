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
    public class SegmentLeaksController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public SegmentLeaksController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SegmentLeaksEntry>>> Get()
        {

            return await _db.SegmentLeaks.ToListAsync();
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SegmentLeaksEntry>> Get(int id)
        {
            var leaks = await _db.SegmentLeaks.FindAsync(id);
            
            if(leaks == null)
            {
                return NotFound();
            }

            return leaks;
        }
        /*
                [HttpGet]
                public async Task<ActionResult<ICollection<MonitorsEntry>>> Get2()
                {

                    return await _db.Monitors.ToListAsync();
                }
                */
        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] SegmentLeaksEntry value)
        {
            await _db.SegmentLeaks.AddAsync(value);
            await _db.SaveChangesAsync();

            ;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] SegmentLeaksEntry value)
        {
            var entry = await _db.SegmentLeaks.FindAsync(id);
            entry = value;
            await _db.SaveChangesAsync();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var entry = await _db.SegmentLeaks.FindAsync(id);
            _db.SegmentLeaks.Remove(entry);
            await _db.SaveChangesAsync();
        }

        [HttpPatch("{id}")]
        public async Task Patch([FromBody] SegmentLeaksEntry value)
        {
            var entry = _db.SegmentLeaks.FirstOrDefault(segL => segL.Id == value.Id);

            if(entry != null)
            {
                entry.LatestTimeStamp = value.LatestTimeStamp;
               await _db.SaveChangesAsync();
            }
            
        }
    }
}