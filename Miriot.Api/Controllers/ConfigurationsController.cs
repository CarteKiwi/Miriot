using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Miriot.Api.Models;
using Miriot.Common.Model;
using System;
using System.Collections.Generic;
using System.Linq;

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

        [HttpPost]
        public IActionResult Post([FromBody]MiriotConfiguration config)
        {
            try
            {
                if (config == null || !ModelState.IsValid)
                {
                    return BadRequest();
                }
                bool itemExists = _context.Configurations.Any(c => c.Id == config.Id);
                if (itemExists)
                {
                    return StatusCode(StatusCodes.Status409Conflict, "config already exists");
                }

                _context.Add(config);
                _context.SaveChanges();
            }
            catch (Exception)
            {
                return BadRequest("Could not create item");
            }

            return Ok(config);
        }


        [HttpPut("{id}")]
        public void Put(int id, [FromBody]MiriotConfiguration config)
        {
            var existing = _context.Configurations
                .Include(u => u.Widgets)
                .Single(c => c.Id == config.Id);

            existing.Widgets.Clear();
            _context.SaveChanges();
            existing.Widgets.AddRange(config.Widgets);

            _context.Entry(existing).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
            _context.Entry(existing).CurrentValues.SetValues(config);

            _context.SaveChanges();
        }

        // DELETE api/values/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
