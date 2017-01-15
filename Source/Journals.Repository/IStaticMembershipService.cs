using System.Web.Security;
using Journals.Model;

namespace Journals.Repository
{
    public interface IStaticMembershipService
    {
        MembershipUser GetUser();

        UserProfile GetUserProfile(int userId);

        bool IsUserInRole(string userName, string roleName);
    }
}