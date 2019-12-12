using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace FreelancerProjectAPI.Models
{
    public class DBInitializer
    {
        public static void Initialize(DatabaseContext context)
        {
            context.Database.EnsureCreated();
            if (context.Users.Any())
            {
                return;   // DB has been seeded
            }

            Status status = new Status()
            {
                CurrentStatus = "Draft"
            };

            context.Status.Add(status);

            status = new Status()
            {
                CurrentStatus = "Finished"
            };

            context.Status.Add(status);

            status = new Status()
            {
                CurrentStatus = "Closed"
            };

            context.Status.Add(status);

            status = new Status()
            {
                CurrentStatus = "Open"
            };

            context.Status.Add(status);

            UserType userType = new UserType()
            {
                Type = "user"
            };

            context.UserTypes.Add(userType);

            userType = new UserType()
            {
                Type = "recruiter"
            };

            context.UserTypes.Add(userType);

            userType = new UserType()
            {
                Type = "admin"
            };

            context.UserTypes.Add(userType);

            User user = new User() {
                Username = "Admin",
                Email = "admin@admin.com",
                Password = "admin",
                LastName = "Tester",
                Name = "Test",
                Bio = "This my admin bio",
                BirthYear = 1999,
                UserType = userType,
                ContactInfo=new ContactInfo
                {
                    MobileNumber = "+3278596204",
                    LinkedIn= "https://www.linkedin.com/in/jeffvdbroeck/"
                },
                Location = new Location()
                {
                    Country="Belgium",
                    Postcode="BE2440",
                    Address="Teststreet 123"
                },
                Image=""

            };
            user.UserSkills = new List<UserSkill>()
                {
                    new UserSkill()
                    {
                        User = user,
                        Skill=new Skill()
                    {
                        SkillName = "C#"
                    }
                    },
                    new UserSkill()
                    {
                        User = user,
                        Skill=new Skill()
                    {
                        SkillName = "PHP"
                    }
                } 
            };

            user.TagUsers = new List<TagUser>
            {
                new TagUser()
                {
                    Tag=new Tag()
                    {
                        TagName="Student"
                    },
                    User=user
                },
                new TagUser()
                {
                    Tag=new Tag()
                    {
                        TagName="New"
                    },
                    User=user
                }
            };

            Company company = new Company()
            {
                CompanyName = "TestCompany",
                About = "This is a test company",
                ContactInfo = new ContactInfo
                {
                    MobileNumber = "895243",
                    LinkedIn = "https://www.linkedin.com/in/jeffvdbroeck/"
                },
                Location = new Location()
                {
                    Country = "Germany",
                    Postcode = "GE1234",
                    Address = "Companystreet 123"
                },Image=""

            };

            UserCompany userCompany = new UserCompany()
            {
                User = user,
                Company = company
            };

            user.Reviews = new List<Review>()
                {
                    new Review()
                    {
                        Score=8.8,
                        Description="Very good work",
                        Title="Excellent!",
                        Company=company
                    },
                    new Review()
                    {
                        Score=2,
                        Description="Bad",
                        Title="Scam!",
                        Company = company
                    }
            };
            company.Reviews = new List<Review>()
                {
                    new Review()
                    {
                        Score=7.6,
                        Description="Very good work",
                        Title="Excellent!",
                        User=user
                    },
                    new Review()
                    {
                        Score=0,
                        Description="Bad",
                        Title="Scam!",
                        User=user
                    }
                };



            Assignment assignment = new Assignment()
            {
                Description = "This is a test description",
                AssignmentName = "TestAssignment",
                Location = new Location()
                {
                    Country = "France",
                    Postcode = "FR440",
                    Address = "Teststreet 12893"
                },
                Company = company
                ,Status=status,Image=""
            };
            assignment.TagAssignments = new List<TagAssignment>()
            {
                new TagAssignment()
                {
                    Tag=new Tag()
                    {
                        TagName="TestTag"
                    },
                    Assignment = assignment
                }
            };

            UserAssignment ua = new UserAssignment()
            {
                User = user,
                Accepted = false,
                Assignment = assignment
            };
            assignment.UserAssignments = new List<UserAssignment>();
            assignment.UserAssignments.Add(ua);

            user.UserAssignments= new List<UserAssignment>() { ua };
            company.Assignments = new List<Assignment>() { assignment };

            user.UserCompanies = new List<UserCompany>() { userCompany };
            company.UserCompanies = new List<UserCompany>() { userCompany };

            context.Users.Add(user);

            context.SaveChanges();
        }
    }
}
