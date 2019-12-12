using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancerProjectAPI.Models;
using Microsoft.AspNetCore.Authorization;

namespace FreelancerProjectAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SkillController : ControllerBase
    {
        private readonly DatabaseContext _context;

        public SkillController(DatabaseContext context)
        {
            _context = context;
        }

        // GET: api/Skill
        [Authorize]
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Skill>>> GetSkillsNotFromUser(long id)
        {
            //var skills = await _context.Skills.ToListAsync();
            var skills = await _context.Skills.ToListAsync();

            List<Skill> rSkills = new List<Skill>();
            

            foreach(Skill s in skills)
            {
                var userSkill = await _context.UserSkills.Where(us => us.Skill.SkillID == s.SkillID).FirstOrDefaultAsync();

                if(userSkill == null)
                {
                    rSkills.Add(s);
                }
            }

            return rSkills;
        }

        [Authorize]
        [HttpDelete("userSkill/{id}")]
        public async Task<ActionResult<UserSkill>> DeleteUserSkill(long id)
        {
            var userSkill = await _context.UserSkills.FindAsync(id);
            if (userSkill == null)
            {
                return NotFound();
            }

            _context.UserSkills.Remove(userSkill);
            await _context.SaveChangesAsync();

            return userSkill;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<Skill>> PostSkill(Skill skill)
        {
            if (_context.Skills.FirstOrDefault(t => t.SkillName == t.SkillName) == null)
            {
                Category category = _context.Categories.Find(skill.Category.CategoryID);
                skill.Category = category;
                _context.Skills.Add(skill);
                await _context.SaveChangesAsync();
                return CreatedAtAction("GetSkill", new { id = skill.SkillID }, skill);
            }
            else
            {
                return NotFound();
            }

        }

    }
}
