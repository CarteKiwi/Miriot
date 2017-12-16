using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
            var user = _context.Users.FirstOrDefault(u => u.Id == id);

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
            var old = _context.Users.FirstOrDefault(u => u.Id == user.Id);

            if(old == null)
            {
                _context.Users.Add(user);
            }
            else
            {
                old = user;
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
