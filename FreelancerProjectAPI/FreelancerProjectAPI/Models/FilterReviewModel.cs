using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class FilterReviewModel
    {
        public string Username { get; set; }
        public string Company { get; set; }
        public string Title { get; set; }
        public bool UserReview { get; set; }
    }
}
