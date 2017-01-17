using Journals.Model;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq.Expressions;
using Microsoft.AspNet.Identity.EntityFramework;
using IdentityUser = Microsoft.AspNet.Identity.CoreCompat.IdentityUser;

namespace Journals.Repository.DataContext
{
    public class JournalsContext : DbContext, IDisposedTracker
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="JournalsContext" /> class.
        /// </summary>
        /// <param name="connectionStringOrName">Name of the connection string or.</param>
        public JournalsContext(string connectionStringOrName)
                    : base(connectionStringOrName)
        {
        }

        /// <summary>
        /// Provides access to the <see cref="Journals"/> table
        /// </summary>
        public IDbSet<Journal> Journals { get; set; }

        /// <summary>
        /// Provides access to the <see cref="Subscriptions"/> table
        /// </summary>
        public IDbSet<Subscription> Subscriptions { get; set; }

        /// <summary>
        /// Provides access to the <see cref="Files"/> table
        /// </summary>

        public IDbSet<File> Files { get; set; }

        /// <summary>
        /// Provides access to the <see cref="Issues"/> table
        /// </summary>
        public IDbSet<Issue> Issues { get; set; }


        /// <inheritdoc cref="IDisposedTracker.IsDisposed"/>
        public bool IsDisposed { get; set; }

        /// <inheritdoc cref="DbContext.OnModelCreating"/>
        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            base.Configuration.LazyLoadingEnabled = false;

            modelBuilder.Entity<Journal>().ToTable("Journals");
            modelBuilder.Entity<Subscription>().ToTable("Subscriptions");
            modelBuilder.Entity<File>().ToTable("Files");
            modelBuilder.Entity<Issue>().ToTable("Issues");

            modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers");
            modelBuilder.Entity<IdentityUser>().ToTable("AspNetUsers");
            modelBuilder.Entity<ApplicationRole>().ToTable("AspNetRoles");
            modelBuilder.Entity<IdentityUserRole>().ToTable("AspNetUserRoles");
            modelBuilder.Entity<IdentityRole>().ToTable("AspNetRoles");


            base.OnModelCreating(modelBuilder);
        }

        /// <inheritdoc cref="DbContext.Dispose(bool)"/>
        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}
