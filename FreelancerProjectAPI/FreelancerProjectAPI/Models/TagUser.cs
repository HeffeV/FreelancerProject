using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class TagUser
    {
        public long TagUserID { get; set; }
        public Tag Tag { get; set; }
        public User User { get; set; }
    }
}
