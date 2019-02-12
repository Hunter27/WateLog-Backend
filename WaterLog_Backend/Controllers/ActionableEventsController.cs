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
    public class ActionableEventsController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        private IControllerService _service;
        public ActionableEventsController(DatabaseContext context, IConfiguration config,IControllerService service)
        {
            _db = context;
            _config = config;
            _service = service;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ActionableEvent>>> Get()
        {

            return await _db.ActionableEvent.ToListAsync();
        }


        [Route("costs/{id}")]
        public async Task<ActionResult<string>> GetCost(int id)
        {
            ActionableEvent leaks = await _db.ActionableEvent.FindAsync(id);
            if (leaks == null)
            {
                return NotFound();
            }
            Procedures procedures = new Procedures(_service);
            return ("{total: " + procedures.calculateTotalCost(leaks) + ", perhour: " +procedures.calculatePerHourCost(leaks) + "}");
        }

        [Route("litres/{id}")]
        public async Task<ActionResult<string>> GetLitres(int id)
        {
            ActionableEvent leaks = await _db.ActionableEvent.FindAsync(id);
            if(leaks == null)
            {
                return NotFound();
            }
            Procedures procedures = new Procedures(_service);
            return ("{total: " + procedures.calculateTotaLitres(leaks) + ", perhour: " + procedures.calculateLitresPerHour(leaks) + "}");
        }

        //Resolve Leakage
        [HttpPost("resolve")]
        public async Task<ActionResult<ActionableEvent>> Resolve([FromForm] int id)
        {
            var leaks = await _db.ActionableEvent.FindAsync(id);
            if(leaks == null)
            {
                return NotFound();
            }
            leaks.Status = "resolved";
            await _db.SaveChangesAsync();
            return leaks;

        }
        // GET api/values/5
        [HttpGet("{id}")]
        public async Task<ActionResult<ActionableEvent>> Get(int id)
        {
            var leaks = await _db.ActionableEvent.FindAsync(id);
            
            if(leaks == null)
            {
                return NotFound();
            }

            return leaks;
        }
       
        // POST api/values
        [HttpPost]
        public async Task Post([FromBody] ActionableEvent value)
        {
            await _db.ActionableEvent.AddAsync(value);
            await _db.SaveChangesAsync();

            ;
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] ActionableEvent value)
        {
            var entry = await _db.ActionableEvent.FindAsync(id);
            entry = value;
            await _db.SaveChangesAsync();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            var entry = await _db.ActionableEvent.FindAsync(id);
            _db.ActionableEvent.Remove(entry);
            await _db.SaveChangesAsync();
        }

        [HttpPatch("{id}")]
        public async Task Patch([FromBody] ActionableEvent value)
        {
            var entry = _db.ActionableEvent.FirstOrDefault(segL => segL.Id == value.Id);

            if(entry != null)
            {
                entry.LatestTimeStamp = value.LatestTimeStamp;
               await _db.SaveChangesAsync();
            }
            
        }
    }
}