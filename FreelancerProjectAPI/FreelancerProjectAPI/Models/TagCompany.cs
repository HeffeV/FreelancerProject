using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class TagCompany
    {
        public long TagCompanyID { get; set; }
        public Tag Tag { get; set; }
        public Company Company { get; set; }
    }
}
