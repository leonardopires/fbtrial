using Journals.Model;
using System.Linq.Expressions;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Journals.Repository.DataContext
{
    public class JournalsContext : ApplicationDbContext, IDisposedTracker
    {

        public JournalsContext(DbContextOptions<JournalsContext> options) : base(options)
        {
        }

        /// <summary>
        /// Provides access to the <see cref="Journals"/> table
        /// </summary>
        public DbSet<Journal> Journals { get; set; }

        /// <summary>
        /// Provides access to the <see cref="Subscriptions"/> table
        /// </summary>
        public DbSet<Subscription> Subscriptions { get; set; }
        
        /// <summary>
        /// Provides access to the <see cref="Files"/> table
        /// </summary>

        public DbSet<File> Files { get; set; }

        /// <summary>
        /// Provides access to the <see cref="Issues"/> table
        /// </summary>
        public DbSet<Issue> Issues { get; set; }


        /// <inheritdoc cref="IDisposedTracker.IsDisposed"/>
        public bool IsDisposed { get; set; }

        /// <inheritdoc cref="DbContext.OnModelCreating"/>
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {

            base.OnModelCreating(modelBuilder);


            modelBuilder.Entity<Journal>().ToTable("Journals").Ignore(j => j.User);
            modelBuilder.Entity<Subscription>().ToTable("Subscriptions").Ignore(j => j.User);
            modelBuilder.Entity<File>().ToTable("Files");
            modelBuilder.Entity<Issue>().ToTable("Issues");

//            modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers").HasKey(k => k.Id);
//            modelBuilder.Entity<IdentityUser>().ToTable("AspNetUsers").HasKey(k => k.Id);
//            modelBuilder.Entity<ApplicationRole>().ToTable("AspNetRoles").HasKey(k => k.Id);
//            modelBuilder.Entity<IdentityUserRole<string>>().ToTable("AspNetUserRoles").HasKey(k => new { k.RoleId, k.UserId });
//            modelBuilder.Entity<IdentityRole>().ToTable("AspNetRoles").HasKey(k => k.Id);
            
        }

        public override void Dispose()
        {
            IsDisposed = true;
            base.Dispose();
        }
    }
}

