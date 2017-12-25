using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Miriot.Api.Models;
using Miriot.Common.Model;

namespace Miriot.Api.Controllers
{
    [Route("api/[controller]")]
    public class ConfigurationsController : Controller
    {
        private readonly MiriotContext _context;

        public ConfigurationsController(MiriotContext context)
        {
            _context = context;
        }

        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            if (_context.Users.Any())
            {
                return _context.Users.Select(u => u.Name);
            }
            else
                return new string[] { "value1", "value2", };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public IActionResult Get(Guid id)
        {
            var w = _context.Configurations.Find(id);
            return Ok(w);
        }

        // POST api/values
        [HttpPost]
        public IActionResult Post([FromBody]User user)
        {
            try
            {
                if (user == null || !ModelState.IsValid)
                {
                    return BadRequest();
                }
                bool itemExists = _context.Users.Any(u => u.Id == user.Id);
                if (itemExists)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "user already exists");
                }

                _context.Add(user);
                _context.SaveChanges();
            }
            catch (Exception)
            {
                return BadRequest("Could not create item");
            }

            return Ok(user);
        }

        // PUT api/values/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody]MiriotConfiguration config)
        {
            var existing = _context.Configurations.Find(config.Id);

            if (existing == null)
            {
                _context.Configurations.Add(config);
            }
            else
            {
                existing.Widgets = config.Widgets;
                //_context.Update(existing);
                //existingUser.Devices = user.Devices;
                //existingUser.ToothbrushingHistory = user.ToothbrushingHistory;
                //_context.Entry(existingUser).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                //_context.Entry(existingUser).CurrentValues.SetValues(user);
            }
            _context.SaveChanges();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
