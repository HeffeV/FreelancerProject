using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class User
    {
        public long UserID { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string Username { get; set; }
        public string Name { get; set; }
        public string LastName { get; set; }
        public string Bio { get; set; }
        public int BirthYear { get; set; }
        public string Image { get; set; }
        [NotMapped]
        public string Token { get; set; }

        public UserType UserType { get; set; }
        public ICollection<Skill> Skills { get; set; }
        public ICollection<Review> Reviews { get; set; }
        public ContactInfo ContactInfo { get; set; }
        public ICollection<UserCompany> UserCompanies {get;set;}
        public ICollection<TagUser> TagUsers { get; set; }
        public ICollection<UserAssignment> UserAssignments { get; set; }
        public Location Location { get; set; }
    }
}
