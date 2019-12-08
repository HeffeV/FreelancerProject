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

            UserType userType = new UserType()
            {
                Type = "user"
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
                Skills = new List<Skill>()
                {
                    new Skill()
                    {
                        SkillName = "C#"
                    },
                    new Skill()
                    {
                        SkillName = "PHP"
                    }
                },
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
                Tags = new List<Tag>
                {
                    new Tag()
                    {
                        TagName="Student"
                    },
                    new Tag()
                    {
                        TagName="New"
                    }
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
                }

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
                AssingmentName = "TestAssignment",
                Location = new Location()
                {
                    Country = "France",
                    Postcode = "FR440",
                    Address = "Teststreet 12893"
                },
                Company = company,
                User=user,
                Tags = new List<Tag>
                {
                    new Tag()
                    {
                        TagName="TestTag"
                    }
                }
            };

            user.Assignments= new List<Assignment>() { assignment };
            company.Assignments = new List<Assignment>() { assignment };

            user.UserCompanies = new List<UserCompany>() { userCompany };
            company.UserCompanies = new List<UserCompany>() { userCompany };

            context.Users.Add(user);

            context.SaveChanges();
        }
    }
}
