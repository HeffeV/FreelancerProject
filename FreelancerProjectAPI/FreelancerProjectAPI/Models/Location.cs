using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class Location
    {
        public long LocationID { get; set; }
        public string Country { get; set; }
        public string Postcode { get; set; }
        public string Address { get; set; }
    }
}
