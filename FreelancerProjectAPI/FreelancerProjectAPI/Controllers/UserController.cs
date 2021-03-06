﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancerProjectAPI.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using System.Net.Mail;
using System.Net;
using System.Text;

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
                    .ThenInclude(uc => uc.Company)
                .Include(u => u.TagUsers).ThenInclude(u => u.Tag)
                .Include(u => u.UserAssignments)
                    .ThenInclude(ua => ua.Assignment)
                        .ThenInclude(a => a.Status)
                .Include(u => u.UserAssignments).ThenInclude(ua => ua.User)
                .Include(u => u.Location)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                return NotFound();
            }

            return user;
        }

        [Authorize]
        [HttpPost("filteredUsers")]
        public async Task<ActionResult<IEnumerable<User>>> GetFilteredUsers(FilterUserModel filterUserModel)
        {
            List<User> users = await _context.Users.Include(u => u.UserType)
                .Include(u => u.UserSkills).ThenInclude(u => u.Skill).ThenInclude(s => s.Category)
                .Include(u => u.ContactInfo)
                .Include(u => u.TagUsers).ThenInclude(u => u.Tag)
                .Include(u => u.Location).ToListAsync();

            if (filterUserModel.Email != null && filterUserModel.Email != "" && filterUserModel.Email != " ")
            {
                users = users.Where(u => u.Email.ToLower().Contains(filterUserModel.Email.ToLower())).ToList();
            }
            if(filterUserModel.Username != null && filterUserModel.Username != "" && filterUserModel.Username != " ")
            {
                users = users.Where(u => u.Username.ToLower().Contains(filterUserModel.Username.ToLower())).ToList();
            }
            if(filterUserModel.UserType != null && filterUserModel.UserType != "")
            {
                users = users.Where(u => u.UserType.Type== filterUserModel.UserType).ToList();
            }

            return users;
        }

        // PUT: api/User/5
        [Authorize]
        [HttpPut]
        public async Task<IActionResult> PutUser([FromBody]User user)
        {
            UserType userType = _context.UserTypes.FirstOrDefault(e => e.Type == user.UserType.Type);

            User tmpUser = await _context.Users
                .Include(u => u.UserSkills).ThenInclude(us => us.Skill)
                .Include(u => u.UserSkills).ThenInclude(us => us.User)
                .Include(u => u.ContactInfo)
                .Include(u => u.TagUsers).ThenInclude(u => u.Tag)
                .Include(u => u.Location)
                .FirstOrDefaultAsync(u => u.UserID == user.UserID);

            tmpUser.UserType = userType;
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
            tmpUser.Password = user.Password;
            tmpUser.Image = user.Image;

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
                .Include(u => u.UserSkills)
                .Include(u => u.ContactInfo)
                .Include(u => u.TagUsers)
                .Include(u => u.UserAssignments)
                .Include(u => u.Location)
                .Include(u=>u.UserCompanies)
                .Include(u=>u.Reviews)
                .FirstOrDefaultAsync(u => u.UserID == id);

            if (user == null)
            {
                return NotFound();
            }

            foreach(UserSkill userSkill in user.UserSkills)
            {
                _context.UserSkills.Remove(userSkill);
            }

            foreach(TagUser tagUser in user.TagUsers)
            {
                _context.TagUsers.Remove(tagUser);
            }

            foreach(UserCompany userCompany in user.UserCompanies)
            {
                _context.UserCompanies.Remove(userCompany);
            }

            foreach(UserAssignment userAssignment in user.UserAssignments)
            {
                _context.UserAssignments.Remove(userAssignment);
            }

            foreach(Review review in user.Reviews)
            {
                _context.Reviews.Remove(review);
            }

            _context.ContactInfos.Remove(user.ContactInfo);
            _context.Locations.Remove(user.Location);
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok();
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

        // PUT: api/User/resetPassword/email
        [HttpPut("resetPassword/{email}")]
        public async Task<IActionResult> ResetPassword(string email)
        {
            User user = await _context.Users.Where(u => u.Email == email).FirstOrDefaultAsync();

            if(user == null)
            {
                return NotFound();
            }

            //random pass
            user.Password = CreatePassword(15);

            //mail
            MailAddress to = new MailAddress(user.Email);
            MailAddress from = new MailAddress("no_reply@subplementum.com");

            MailMessage message = new MailMessage(from, to);

            SmtpClient client = new SmtpClient("smtp.mailtrap.io", 2525)
            {
                Credentials = new NetworkCredential("e858272f0a8ade", "e07e9c741f59a8"),
                EnableSsl = true
            };

            message.Subject = "Password reset";
            message.Body = "<html><body style='font-family: Arial;color:#818296;'><div style='width:100%; padding:5px; background-color: #10ABFE; color: white'><h2>Password reset</h2></div><p>Your password has been reset. Below you can find your new password. You can change it in your account settings.</p><p style='padding-bottom:15px;'>New password: <strong>" + user.Password+ "</strong></p><a style='background-color: #10ABFE; color:white; padding:10px; text-decoration: none;' href='https://subplementum-fp.firebaseapp.com'>Login to your account</a></body></html>";

            message.IsBodyHtml = true;

            try
            {
                client.Send(message);
            }
            catch (SmtpException ex)
            {
                return BadRequest(ex);
            }

            //update
            _context.Entry(user).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return Ok();
        }

        private string CreatePassword(int length)
        {
            const string valid = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890";
            StringBuilder res = new StringBuilder();
            Random rnd = new Random();
            while (0 < length--)
            {
                res.Append(valid[rnd.Next(valid.Length)]);
            }
            return res.ToString();
        }
    }
}
