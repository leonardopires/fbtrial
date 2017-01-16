using System;
using System.Threading.Tasks;
using Journals.Model;
using Journals.Web.Models;

namespace Journals.Repository
{
    public interface IStaticMembershipService : IDisposable
    {
        ApplicationUser GetUser();

        ApplicationUser GetUser(string userId);

        bool IsUserInRole(string userName, string roleName);
    }
}