using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class FilterCompanyModel
    {
        public string Country
        {
            get;set;
        }
        public string CompanyName { get; set; }
        public string Postcode { get; set; }
    }
}
