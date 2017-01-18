using System.Linq;
using System.Threading.Tasks;
using Journals.Model;
using Journals.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace Journals.Repository
{
    public class StaticMembershipService : IStaticMembershipService
    {

        private readonly UserManager<ApplicationUser> identity;
        private readonly IHttpContextAccessor contextAccessor;

        public StaticMembershipService(UserManager<ApplicationUser> identity, IHttpContextAccessor contextAccessor)
        {
            this.identity = identity;
            this.contextAccessor = contextAccessor;
        }

        public ApplicationUser GetUser()
        {
            var user = contextAccessor.HttpContext.User;
            var userId = identity.GetUserId(user);

            return identity.Users.FirstOrDefault(i => i.Id == userId);
        }

        public ApplicationUser GetUser(string userId)
        {
            return Task.Run(() => identity.FindByIdAsync(userId)).Result;
        }
        public bool IsUserInRole(string userName, string roleName)
        {
            var user = identity.Users.FirstOrDefault(u => u.UserName == userName);

            return user != null && Task.Run(() => identity.IsInRoleAsync(user, roleName)).Result;
        }

        public void Dispose()
        { 
            identity?.Dispose();
            identity?.Dispose();
        }

        public ApplicationUser GetUserByName(string userName)
        {
            return Task.Run(() => identity.FindByNameAsync(userName)).Result;
        }

    }
}