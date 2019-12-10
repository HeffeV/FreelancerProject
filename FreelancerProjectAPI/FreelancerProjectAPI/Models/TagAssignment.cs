using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class TagAssignment
    {
        public long TagAssignmentID { get; set; }
        public Tag Tag { get; set; }
        public Assignment Assignment { get; set; }
    }
}
