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
                .Include(u => u.UserSkills).ThenInclude(u => u.Skill)
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

        [HttpGet]
        [Authorize]
        [HttpGet("filteredUsers")]
        public async Task<ActionResult<IEnumerable<User>>> GetFilteredUsers(string email, string username, string usertype)
        {
            List<User> users = await _context.Users.Include(u => u.UserType).ToListAsync();

            if (email != null || email != "")
            {
                users = users.Where(u => u.Email.Contains(email)).ToList();
            }
            if(username != null || username != "")
            {
                users = users.Where(u => u.Username.Contains(username)).ToList();
            }
            if(usertype != null || usertype != "")
            {
                users = users.Where(u => u.UserType.Type==usertype).ToList();
            }

            return users;
        }

        // PUT: api/User/5
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> PutUser([FromBody]User user)
        {
            User tmpUser = await _context.Users
                .Include(u => u.UserSkills).ThenInclude(us => us.Skill)
                .Include(u => u.UserSkills).ThenInclude(us => us.User)
                .Include(u => u.ContactInfo)
                .Include(u => u.TagUsers).ThenInclude(u => u.Tag)
                .Include(u => u.Location)
                .FirstOrDefaultAsync(u => u.UserID == user.UserID);

            tmpUser.Location.Country = user.Location.Country;
            tmpUser.Location.Address = user.Location.Address;
            tmpUser.Location.Postcode = user.Location.Postcode;
            tmpUser.ContactInfo.MobileNumber = user.ContactInfo.MobileNumber;
            tmpUser.ContactInfo.LinkedIn = user.ContactInfo.LinkedIn;
            tmpUser.Email = user.Email;
            tmpUser.BirthYear = user.BirthYear;
            tmpUser.Bio = user.Bio;
            tmpUser.LastName = user.LastName;
            tmpUser.Name = user.Name;
            tmpUser.Username = user.Username;

            foreach (TagUser tc in user.TagUsers)
            {
                TagUser tmpTagUser = _context.TagUsers.Include(t => t.Tag).SingleOrDefault(t => t.Tag.TagName == tc.Tag.TagName && t.TagUserID == tc.TagUserID);
                if (tmpTagUser == null || tmpTagUser.Equals(null))
                {
                    //tag is nog niet toegevoegd aan user
                    Tag tmpTag = _context.Tags.SingleOrDefault(t => t.TagName == tc.Tag.TagName);
                    if (tmpTag == null || tmpTag.Equals(null))
                    {
                        tmpUser.TagUsers.Add(new TagUser() { Tag = new Tag() { TagName = tc.Tag.TagName }, User = tmpUser });
                    }
                }
            }

            foreach (UserSkill us in user.UserSkills)
            {
                UserSkill tmpUserSkill = _context.UserSkills.Include(uss => uss.Skill).SingleOrDefault(uss => uss.Skill.SkillName == us.Skill.SkillName && uss.UserSkillID == us.UserSkillID);
                if (tmpUserSkill == null || tmpUserSkill.Equals(null))
                {
                    //skill is nog niet toegevoegd aan user
                    Skill tmpSkill = _context.Skills.SingleOrDefault(s => s.SkillName == us.Skill.SkillName);
                    if (tmpSkill == null || tmpSkill.Equals(null))
                    {
                        tmpUser.UserSkills.Add(new UserSkill() { User = tmpUser , Skill = new Skill() { SkillName = us.Skill.SkillName }});
                    }
                    else
                    {
                        tmpUser.UserSkills.Add(new UserSkill() { User = tmpUser, Skill = tmpSkill });
                    }
                }

            }

            _context.Entry(tmpUser).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
        }

        // POST: api/User
        [HttpPost]
        public async Task<ActionResult<User>> PostUser(User user)
        {
            if (!_context.Users.Any(e => e.Username == user.Username) && !_context.Users.Any(e => e.Email == user.Email))
            {
                UserType userType = _context.UserTypes.FirstOrDefault(e=>e.Type==user.UserType.Type);
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
            var user = await _context.Users
                .Include(u => u.UserSkills).ThenInclude(u => u.Skill)
                    .ThenInclude(s => s.Category)
                .Include(u => u.ContactInfo)
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

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return user;
        }

        private bool UserExists(long id)
        {
            return _context.Users.Any(e => e.UserID == id);
        }

        [Authorize]
        [HttpPut("updateavatar")]
        public async Task<IActionResult> PutImage(User user)
        {

            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
