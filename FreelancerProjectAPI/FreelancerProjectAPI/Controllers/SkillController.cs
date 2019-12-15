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
            var skills = await _context.Skills.ToListAsync();

            List<Skill> rSkills = new List<Skill>();
            

            foreach(Skill s in skills)
            {
                var userSkill = await _context.UserSkills.Include(us => us.Skill).Where(us => us.Skill.SkillID == s.SkillID).FirstOrDefaultAsync();

                if(userSkill != null)
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
        [HttpPost("PostSkill")]
        public async Task<ActionResult<Skill>> PostSkill(Skill skill)
        {
            if (_context.Skills.FirstOrDefault(t => t.SkillName == skill.SkillName) == null)
            {
                Category category = _context.Categories.FirstOrDefault(s=>s.CategoryName.ToLower()==skill.Category.CategoryName.ToLower());
                if (category == null)
                {
                    return BadRequest();
                }
                skill.Category = category;
                _context.Skills.Add(skill);
                await _context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return NotFound();
            }

        }

        [Authorize]
        [HttpPost("PostCategory")]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            if (_context.Categories.FirstOrDefault(t => t.CategoryName.ToLower() == category.CategoryName.ToLower()) == null)
            {
                Category tmpCategory = new Category() { CategoryName=category.CategoryName};
                _context.Categories.Add(tmpCategory);
                await _context.SaveChangesAsync();
                return Ok();
            }
            else
            {
                return NotFound();
            }

        }

        [Authorize]
        [HttpDelete("DeleteSkill")]
        public async Task<ActionResult<UserSkill>> DeleteSkill(long id)
        {
            var skill = await _context.Skills.FindAsync(id);
            if (skill == null)
            {
                return NotFound();
            }

            var userskills = _context.UserSkills.Where(s => s.Skill.SkillID == id);

            foreach(UserSkill userSkill in userskills)
            {
                _context.UserSkills.Remove(userSkill);
            }

            _context.Skills.Remove(skill);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpDelete("DeleteCategory")]
        public async Task<ActionResult<Category>> DeleteCategory(long id)
        {
            var skills = await _context.Skills.Include(s=>s.Category).Where(e=>e.Category.CategoryID==id).ToListAsync();
            var category = await _context.Categories.FindAsync(id);

            if (category == null)
            {
                return NotFound();
            }

            List<UserSkill> userSkills = new List<UserSkill>();

            foreach(Skill skill in skills)
            {
                userSkills.AddRange(_context.UserSkills.Where(e => e.Skill.SkillID==skill.SkillID));
            }

            foreach (UserSkill userSkill in userSkills)
            {
                _context.UserSkills.Remove(userSkill);
            }

            foreach (Skill skill in skills)
            {
                _context.Skills.Remove(skill);
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();

            return Ok();
        }

        [Authorize]
        [HttpGet("getCategories")]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            var categories = await _context.Categories.ToListAsync();
            return categories;
        }

        [Authorize]
        [HttpGet("getSkills")]
        public async Task<ActionResult<IEnumerable<Skill>>> GetSkill()
        {
            var skills = await _context.Skills.ToListAsync();
            return skills;
        }

    }
}
