using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class UserCompany
    {
        public long UserCompanyID { get; set; }
        public Company Company { get; set; }
        public User User { get; set; }
		public bool Accepted { get; set; }
	}
}
