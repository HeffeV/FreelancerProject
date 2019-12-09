using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace FreelancerProjectAPI.Models
{
    public class DatabaseContext:DbContext
    {
        public DatabaseContext(DbContextOptions<DatabaseContext>options): base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<UserType> UserTypes { get; set; }
        public DbSet<Skill> Skills { get; set; }
        public DbSet<Category> Categories { get; set; }
        public DbSet<Review> Reviews { get; set; }
        public DbSet<ContactInfo> ContactInfos { get; set; }
        public DbSet<Tag> Tags { get; set; }
        public DbSet<Assignment> Assignments {get; set; }
        public DbSet<Location> Locations { get; set; }
        public DbSet<UserCompany> UserCompanies { get; set; }
        public DbSet<Company> Companies { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<User>().ToTable("User");
            modelBuilder.Entity<UserType>().ToTable("UserType");
            modelBuilder.Entity<Skill>().ToTable("Skill");
            modelBuilder.Entity<Category>().ToTable("Category");
            modelBuilder.Entity<Review>().ToTable("Review");
            modelBuilder.Entity<ContactInfo>().ToTable("ContactInfo");
            modelBuilder.Entity<Tag>().ToTable("Tag");
            modelBuilder.Entity<Assignment>().ToTable("Assignment");
            modelBuilder.Entity<Location>().ToTable("Location");
            modelBuilder.Entity<UserCompany>().ToTable("UserCompany");
            modelBuilder.Entity<Company>().ToTable("Company");
        }
    }
}
