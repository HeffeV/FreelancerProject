using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class UserSkill
    {
        public long UserSkillID{get;set;}
        public User User { get; set; }
        public Skill Skill { get; set; }
    }
}
