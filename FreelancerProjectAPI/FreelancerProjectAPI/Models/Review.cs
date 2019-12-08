using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class Review
    {
        public long ReviewID { get; set; }
        public double Score { get; set; }
        public string Description { get; set; }
        public string Title { get; set; }
        public User User { get; set; }
        public Company Company { get; set; }
    }
}
