using System;
using Journals.Model;

namespace Journals.Services
{
    public interface IStaticMembershipService : IDisposable
    {
        ApplicationUser GetUser();

        ApplicationUser GetUser(string userId);

        bool IsUserInRole(string userName, string roleName);

        ApplicationUser GetUserByName(string userName);

    }
}