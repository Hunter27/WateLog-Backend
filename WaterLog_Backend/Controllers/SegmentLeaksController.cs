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
    public class SegmentLeaksController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        private IControllerService _service;
        public SegmentLeaksController(DatabaseContext context, IConfiguration config, IControllerService service)
        {
            _db = context;
            _config = config;
            _service = service;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SegmentLeaksEntry>>> Get()
        {
            return await _db.SegmentLeaks.ToListAsync();
        }


        [Route("costs/{id}")]
        public async Task<ActionResult<string>> GetCost(int id)
        {
            SegmentLeaksEntry leaks = await _db.SegmentLeaks.FindAsync(id);
            if (leaks == null)
            {
                return NotFound();
            }
            Procedures procedures = new Procedures(_db, _config);
            return (JsonConvert.SerializeObject((procedures.calculateTotalCost(leaks), procedures.calculatePerHourCost(leaks))));
        }

        [Route("litres/{id}")]
        public async Task<ActionResult<string>> GetLitres(int id)
        {
            SegmentLeaksEntry leaks = await _db.SegmentLeaks.FindAsync(id);
            if (leaks == null)
            {
                return NotFound();
            }
            Procedures procedures = new Procedures(_db, _config);
            return (JsonConvert.SerializeObject((procedures.calculateTotaLitres(leaks), procedures.calculateLitresPerHour(leaks))));
        }

        //Resolve Leakage
        [HttpPost("resolve")]
        public async Task<ActionResult<SegmentLeaksEntry>> Resolve([FromForm] int id)
        {
            var leaks = await _db.SegmentLeaks.FindAsync(id);
            if (leaks == null)
            {
                return NotFound();
            }
            leaks.ResolvedStatus = "resolved";
            await _db.SaveChangesAsync();
            return leaks;
        }
        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<SegmentLeaksEntry>> Get(int id)
        {
            var leaks = await _db.SegmentLeaks.FindAsync(id);

            if (leaks == null)
            {
                return NotFound();
            }
            return leaks;
        }

        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] SegmentLeaksEntry value)
        {
            await _db.SegmentLeaks.AddAsync(value);
            await _db.SaveChangesAsync();
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] SegmentLeaksEntry value)
        {
            var old = await _db.SegmentLeaks.FindAsync(id);
            _db.Entry(old).CurrentValues.SetValues(value);
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

            if (entry != null)
            {
                entry.LatestTimeStamp = value.LatestTimeStamp;
                await _db.SaveChangesAsync();
            }
        }
    }
}
