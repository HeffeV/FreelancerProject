using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class Skill
    {
        public long SkillID { get; set; }
        public string SkillName { get; set; }
        public Category Category { get; set; }
    }
}
