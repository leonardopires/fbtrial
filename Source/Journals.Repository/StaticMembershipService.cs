using System.Web.Security;
using Journals.Model;
using WebMatrix.WebData;

namespace Journals.Repository
{
    public class StaticMembershipService : IStaticMembershipService
    {

        public System.Web.Security.MembershipUser GetUser()
        {
            return Membership.GetUser();
        }

        public UserProfile GetUserProfile(int userId)
        {
            var user = Membership.GetUser(userId);
            return new UserProfile() {UserId = (int) (user.ProviderUserKey ?? -1), UserName = user.UserName};
        }
        public bool IsUserInRole(string userName, string roleName)
        {
            var roles = (SimpleRoleProvider)Roles.Provider;
            return roles.IsUserInRole(userName, roleName);
        }
    }
}