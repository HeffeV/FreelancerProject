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
                CurrentStatus = "Open"
            };

            context.Status.Add(status);

            UserType userType = new UserType()
            {
                Type = "user"
            };

            User user = new User()
            {
                Username = "User",
                Email = "user@user.com",
                Password = "user",
                LastName = "Tester",
                Name = "Test",
                Bio = "This my user bio",
                BirthYear = 1999,
                UserType = userType,
                ContactInfo = new ContactInfo
                {
                    MobileNumber = "+3278596204",
                    LinkedIn = "https://www.linkedin.com/in/jeffvdbroeck/"
                },
                Location = new Location()
                {
                    Country = "Belgium",
                    Postcode = "BE2440",
                    Address = "Userstreet 123"
                },
                Image = ""

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
                },
                Image = ""

            };

            user.Reviews = new List<Review>()
                {
                    new Review()
                    {
                        Score=8.8,
                        Description="Very good work",
                        Title="Excellent!",
                        Company=company,
                        User=user
                        ,UserReview=false
                    },
                    new Review()
                    {
                        Score=2,
                        Description="Bad",
                        Title="Scam!",
                        User=user,
                        Company = company
                        ,UserReview=false
                    }
            };

            Assignment assignment = new Assignment()
            {
                Description = "This is an open description",
                AssignmentName = "OpenAssignment",
                Location = new Location()
                {
                    Country = "France",
                    Postcode = "FR440",
                    Address = "Teststreet 12893"
                },
                Company = company
                ,
                Status = status,
                Image = ""
            };

            assignment.TagAssignments = new List<TagAssignment>()
            {
                new TagAssignment()
                {
                    Tag=new Tag()
                    {
                        TagName="Open"
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

            user.UserAssignments = new List<UserAssignment>() { ua };
            company.Assignments = new List<Assignment>() { assignment };

            //)--------------------
            status = new Status()
            {
                CurrentStatus = "Finished"
            };
            context.Status.Add(status);
            Assignment finishedassignment = new Assignment()
            {
                Description = "This is a finished assignment description",
                AssignmentName = "FinishedAssignment",
                Location = new Location()
                {
                    Country = "United Kingdom",
                    Postcode = "UK8080",
                    Address = "Teststreet 12893"
                },
                Company = company
,
                Status = status,
                Image = ""
            };

            finishedassignment.TagAssignments = new List<TagAssignment>()
            {
                new TagAssignment()
                {
                    Tag=new Tag()
                    {
                        TagName="Finished"
                    },
                    Assignment = finishedassignment
                }
            };

            UserAssignment finishedua = new UserAssignment()
            {
                User = user,
                Accepted = true,
                Assignment = finishedassignment
            };

            finishedassignment.UserAssignments = new List<UserAssignment>();
            finishedassignment.UserAssignments.Add(finishedua);

            user.UserAssignments.Add(finishedua);
            company.Assignments.Add(finishedassignment);

            status = new Status()
            {
                CurrentStatus = "Closed"
            };

            context.Status.Add(status);

            Assignment closedassignment = new Assignment()
            {
                Description = "This is a closed assignment description",
                AssignmentName = "ClosedAssignment",
                Location = new Location()
                {
                    Country = "Italy",
                    Postcode = "IT8080",
                    Address = "Teststreet 12893"
                },
                Company = company
    ,
                Status = status,
                Image = ""
            };

            closedassignment.TagAssignments = new List<TagAssignment>()
            {
                new TagAssignment()
                {
                    Tag=new Tag()
                    {
                        TagName="Closed"
                    },
                    Assignment = closedassignment
                }
            };

            UserAssignment closedua = new UserAssignment()
            {
                User = user,
                Accepted = true,
                Assignment = closedassignment
            };

            closedassignment.UserAssignments = new List<UserAssignment>();
            closedassignment.UserAssignments.Add(closedua);

            user.UserAssignments.Add(closedua);
            company.Assignments.Add(closedassignment);

            status = new Status()
            {
                CurrentStatus = "Draft"
            };

            context.Status.Add(status);

            Assignment draftassignment = new Assignment()
            {
                Description = "This is a draft description",
                AssignmentName = "DraftAssignment",
                Location = new Location()
                {
                    Country = "Spain",
                    Postcode = "SP440",
                    Address = "Teststreet 12893"
                },
                Company = company
                ,
                Status = status,
                Image = ""
            };

            draftassignment.TagAssignments = new List<TagAssignment>()
            {
                new TagAssignment()
                {
                    Tag=new Tag()
                    {
                        TagName="Gaming"
                    },
                    Assignment = draftassignment
                }
            };

            company.Assignments.Add(draftassignment);


            context.UserTypes.Add(userType);

            userType = new UserType()
            {
                Type = "recruiter"
            };

            User recruiter = new User()
            {
                Username = "Recruiter",
                Email = "recruiter@recruiter.com",
                Password = "recruiter",
                LastName = "Test recruiter",
                Name = "Test recruiter name",
                Bio = "This my recruiter bio",
                BirthYear = 1980,
                UserType = userType,
                ContactInfo = new ContactInfo
                {
                    MobileNumber = "+4858584",
                    LinkedIn = "https://www.linkedin.com/in/jeffvdbroeck/"
                },
                Location = new Location()
                {
                    Country = "Belgium",
                    Postcode = "BE2450",
                    Address = "recruiterstreet 123"
                },
                Image = ""

            };

			UserCompany recruiterCompany = new UserCompany()
			{
				User = recruiter,
				Company = company,
				Accepted = true
            };

            company.Reviews = new List<Review>()
                {
                    new Review()
                    {
                        Score=7.6,
                        Description="Very good work",
                        Title="Excellent!",
                        User=user,
                        Company = company
                        ,UserReview=true
                    },
                    new Review()
                    {
                        Score=0,
                        Description="Bad",
                        Title="Scam!",
                        User=user,
                        Company = company
                        ,UserReview=true
                    }
                };


            recruiter.UserCompanies = new List<UserCompany>() { recruiterCompany };
            company.UserCompanies = new List<UserCompany>() { recruiterCompany };

            context.UserTypes.Add(userType);

            userType = new UserType()
            {
                Type = "admin"
            };

            User admin = new User()
            {
                Username = "Admin",
                Email = "admin@admin.com",
                Password = "admin",
                LastName = "Test admin",
                Name = "Test admin name",
                Bio = "This my admin bio",
                BirthYear = 1965,
                UserType = userType,
                ContactInfo = new ContactInfo
                {
                    MobileNumber = "+4858584",
                    LinkedIn = "https://www.linkedin.com/in/jeffvdbroeck/"
                },
                Location = new Location()
                {
                    Country = "Germany",
                    Postcode = "GE7895",
                    Address = "recruiterstreet 123"
                },
                Image = ""

            };

            context.UserTypes.Add(userType);

            context.Users.Add(user);
            context.Users.Add(recruiter);
            context.Users.Add(admin);

            context.SaveChanges();
        }
    }
}
