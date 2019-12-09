using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class UserAssignment
    {
        public long UserAssignmentID { get; set; }
        public User User { get; set; }
        public Assignment Assignment { get; set; }
        public bool Accepted { get; set; }

    }
}
