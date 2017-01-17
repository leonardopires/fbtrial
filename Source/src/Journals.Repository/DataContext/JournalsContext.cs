using Journals.Model;
using System.Data.Entity;
using System.Data.Entity.ModelConfiguration.Conventions;
using System.Linq.Expressions;
using Microsoft.AspNet.Identity.CoreCompat;
using Microsoft.AspNet.Identity.EntityFramework;
using IdentityRole = Microsoft.AspNet.Identity.CoreCompat.IdentityRole;
using IdentityUser = Microsoft.AspNet.Identity.CoreCompat.IdentityUser;
using IdentityUserLogin = Microsoft.AspNet.Identity.CoreCompat.IdentityUserLogin;

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

            base.OnModelCreating(modelBuilder);

            modelBuilder.Conventions.Remove<OneToManyCascadeDeleteConvention>();

            base.Configuration.LazyLoadingEnabled = false;

            modelBuilder.Entity<Journal>().ToTable("Journals");
            modelBuilder.Entity<Subscription>().ToTable("Subscriptions");
            modelBuilder.Entity<File>().ToTable("Files");
            modelBuilder.Entity<Issue>().ToTable("Issues");

            modelBuilder.Entity<ApplicationUser>().ToTable("AspNetUsers").HasKey(k => k.Id);
            modelBuilder.Entity<IdentityUser>().ToTable("AspNetUsers").HasKey(k=>  k.Id);
            modelBuilder.Entity<ApplicationRole>().ToTable("AspNetRoles").HasKey(k => k.Id);
            modelBuilder.Entity<IdentityUserRole>().ToTable("AspNetUserRoles").HasKey(k => new { k.RoleId, k.UserId});
            modelBuilder.Entity<IdentityRole>().ToTable("AspNetRoles").HasKey(k => k.Id);
            modelBuilder.Entity<IdentityUserLogin>().ToTable("AspNetUserLogins").HasKey(k => new { k.UserId, k.LoginProvider});
            modelBuilder.Entity<IdentityUserClaim>().ToTable("AspNetUserClaims").HasKey(k => k.Id );
            modelBuilder.Entity<IdentityRoleClaim>().ToTable("AspNetRoleClaims").HasKey(k => k.Id);
        }

        /// <inheritdoc cref="DbContext.Dispose(bool)"/>
        protected override void Dispose(bool disposing)
        {
            IsDisposed = true;
            base.Dispose(disposing);
        }
    }
}

