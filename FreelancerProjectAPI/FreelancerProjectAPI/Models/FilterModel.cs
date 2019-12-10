using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class FilterModel
    {
        public string Title { get; set; }
        public List<Tag> Tags { get; set; }
        public List<Skill> Skills { get; set; }
        public string CompanyName { get; set; }
        public string Description { get; set; }
    }
}
