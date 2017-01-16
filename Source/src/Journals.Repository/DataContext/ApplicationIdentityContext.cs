using Journals.Model;
using Microsoft.AspNet.Identity.CoreCompat;

namespace Journals.Repository.DataContext
{
    public class ApplicationIdentityContext : IdentityDbContext<ApplicationUser>
    {
        public ApplicationIdentityContext(string connectionString) : base(connectionString)
        {
            
        }

    }
}