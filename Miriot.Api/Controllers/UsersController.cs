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
    public class UsersController : Controller
    {
        private readonly MiriotContext _context;

        public UsersController(MiriotContext context)
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
            var user = _context.Users.AsNoTracking()
                .Include(u => u.Devices)
                .ThenInclude(w => w.Widgets)
                .Include(u => u.ToothbrushingHistory)
                .FirstOrDefault(u => u.Id == id);

            return Ok(user);
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
        public void Put(int id, [FromBody]User user)
        {
            var existingUser = _context.Users.AsNoTracking()
                .FirstOrDefault(u => u.Id == user.Id);

            if (existingUser == null)
            {
                _context.Users.Add(user);
            }
            else
            {
                //foreach (var d in user.Devices)
                //{
                //    d.UserId = user.Id;
                //    _context.Configurations.Update(d);

                //    foreach (var w in d.Widgets)
                //    {
                //        _context.Widgets.Update(w);
                //    }
                //}
                //existingUser.Devices = user.Devices;

                //foreach(var d in user.Devices)
                //{
                //    var config = _context.Configurations.Find(d.Id);

                //    if(config != null)
                //    {
                //        config.Widgets = d.Widgets;
                //        _context.Configurations.Update(config);
                //    }
                //}

                //existingUser.ToothbrushingHistory = user.ToothbrushingHistory;

                //_context.Users.Update(existingUser);
                //_context.Update(user);
                //existingUser.Devices = user.Devices;
                //existingUser.ToothbrushingHistory = user.ToothbrushingHistory;
                _context.Entry(existingUser).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
                _context.Entry(existingUser).CurrentValues.SetValues(user);
                //_context.Users.Update(existingUser);
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
