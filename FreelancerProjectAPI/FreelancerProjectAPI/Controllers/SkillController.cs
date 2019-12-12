using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using FreelancerProjectAPI.Models;

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
    }
}
