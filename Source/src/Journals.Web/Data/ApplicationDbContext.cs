using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journals.Model;
using Microsoft.EntityFrameworkCore;
using Journals.Web.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace Journals.Web.Data
{
    // Add profile data for application users by adding properties to the ApplicationUser class
    public class ApplicationUserCore : IdentityUser
    {
    }

    public class ApplicationRoleCore : IdentityRole
    {
    }

    public class ApplicationDbContext : IdentityDbContext<ApplicationUserCore, ApplicationRoleCore, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);
            // Customize the ASP.NET Identity model and override the defaults if needed.
            // For example, you can rename the ASP.NET Identity table names and more.
            // Add your customizations after calling base.OnModelCreating(builder);
            
        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            base.OnConfiguring(optionsBuilder);                        
        }

    }
}
