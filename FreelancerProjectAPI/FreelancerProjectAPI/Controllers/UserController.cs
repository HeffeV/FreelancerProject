using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancerProjectAPI.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace FreelancerProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public UserController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: api/User
        /*[HttpGet]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            return await _context.Users.ToListAsync();
        }*/

        // GET: api/User/5
        [HttpGet("{id}")]
        public async Task<ActionResult<User>> GetUser(long id)
        {
            var user = await _context.Users
                .Include(u => u.UserType)
                .Include(u => u.UserSkills).ThenInclude(u=>u.Skill)
                    .ThenInclude(s => s.Category)
                .Include(u => u.Reviews)
                    .ThenInclude(r => r.Company)
                .Include(u => u.ContactInfo)
                .Include(u => u.UserCompanies)
                .Include(u => u.TagUsers).ThenInclude(u => u.Tag)
                .Include(u => u.UserAssignments)
                    .ThenInclude(ua => ua.Assignment)
                        .ThenInclude(a => a.Status)
                .Include(u => u.Location)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        // PUT: api/User/5
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> PutUser([FromBody]User user)

        {

            User tmpUser = await _context.Users.Include(u => u.TagUsers).FirstOrDefaultAsync(u => u.UserID == user.UserID);

            foreach (TagUser tu in tmpUser.TagUsers)
            {
                //_context.tag
            }

            _context.Entry(user).State = EntityState.Modified;
            _context.Entry(user.Location).State = EntityState.Modified;
            _context.Entry(user.ContactInfo).State = EntityState.Modified;
            _context.Entry(user.TagUsers).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!UserExists(user.UserID))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // POST: api/User
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (!_context.Users.Any(e => e.Username == user.Username) && !_context.Users.Any(e => e.Email == user.Email))
            {
                UserType userType = _context.UserTypes.Find(user.UserType.UserTypeID);
                user.UserType = userType;
                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetUser", new { id = user.UserID }, user);
            }
            else
            {
                return NotFound();
            }
        }

        // DELETE: api/User/5
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult<User>> DeleteUser(long id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }
    }
}
