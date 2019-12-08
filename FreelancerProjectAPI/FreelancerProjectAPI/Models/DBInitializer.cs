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

            context.SaveChanges();
        }
    }
}
