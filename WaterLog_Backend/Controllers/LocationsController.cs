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
    public class LocationsController : ControllerBase
    {
        private readonly DatabaseContext _db;
        readonly IConfiguration _config;
        public LocationsController(DatabaseContext context, IConfiguration config)
        {
            _db = context;
            _config = config;
        }

        // GET api/values
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocationsEntry>>> Get()
        {
            return await _db.Locations.ToListAsync();
        }

        // GET api/values/
        [HttpGet("{id}")]
        public async Task<ActionResult<LocationsEntry>> Get(int id)
        {
            return await _db.Locations.FindAsync(id);
        }

        // POST api/values
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LocationsEntry value)
        {
            if (ModelState.IsValid)
            {
                await _db.Locations.AddAsync(value);
                await _db.SaveChangesAsync();
                return Ok("Ok");
            }
            else
            {
                return new BadRequestObjectResult("Not Found");
            }

        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public async Task Put(int id, [FromBody] LocationsEntry value)
        {
            try
            {
                var old = await _db.Locations.FindAsync(id);
                _db.Entry(old).CurrentValues.SetValues(value);
                await _db.SaveChangesAsync();
            }
            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        }
        // DELETE api/values/
        [HttpDelete("{id}")]
        public async Task Delete(int id)
        {
            try
            {
                var entry = await _db.Locations.FindAsync(id);
                _db.Locations.Remove(entry);
                await _db.SaveChangesAsync();
            }
            catch (Exception error)
            {
                throw new Exception(error.Message);
            }
        }
    }
}