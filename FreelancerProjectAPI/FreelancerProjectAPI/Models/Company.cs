using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class Company
    {
        public long CompanyID { get; set; }
        public ICollection<UserCompany> UserCompanies{get;set;}
        public ICollection<Review> Reviews { get; set; }
        public ICollection<Assignment> Assignments { get; set; }
        public ICollection<Tag> Tags { get; set; }
        public Location Location { get; set; }
        public string CompanyName { get; set; }
        public ContactInfo ContactInfo { get; set; }
        public string About { get; set; }
    }
}
