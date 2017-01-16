using Journals.Model;

namespace Journals.Repository
{
    public interface IStaticMembershipService
    {
        UserProfile GetUser();

        UserProfile GetUserProfile(int userId);

        bool IsUserInRole(string userName, string roleName);
    }
}