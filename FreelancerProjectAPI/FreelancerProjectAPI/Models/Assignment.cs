using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class Assignment
    {
        public long AssignmentID { get; set; }
        public ICollection<Tag> Tags { get; set; }
        public string Description { get; set; }
        public string AssignmentName { get; set; }
        public Location Location { get; set; }
        public Company Company { get; set; }
        public UserAssignment UserAssignment { get; set; }
        public string Status { get; set; }
    }
}
