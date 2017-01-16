using System.IdentityModel.Claims;
using System.Linq;
using System.Threading.Tasks;
using Journals.Model;
using Journals.Web.Data;
using Journals.Web.Models;
using Classic = Microsoft.AspNet.Identity;
using Core = Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;

namespace Journals.Repository
{
    public class StaticMembershipService : IStaticMembershipService
    {

        private readonly Classic.UserManager<ApplicationUser> identity;
        private readonly Core.UserManager<ApplicationUserCore> coreIdentity;
        private readonly IHttpContextAccessor contextAccessor;

        public StaticMembershipService(Classic.UserManager<ApplicationUser> identity, Core.UserManager<ApplicationUserCore> coreIdentity, IHttpContextAccessor contextAccessor)
        {
            this.identity = identity;
            this.coreIdentity = coreIdentity;
            this.contextAccessor = contextAccessor;
        }

        public ApplicationUser GetUser()
        {
            var user = contextAccessor.HttpContext.User;
            var userId = coreIdentity.GetUserId(user);

            return identity.Users.FirstOrDefault(i => i.Id == userId);
        }

        public ApplicationUser GetUser(string userId)
        {
            return Task.Run(() => identity.FindByIdAsync(userId)).Result;
        }
        public bool IsUserInRole(string userName, string roleName)
        {
            var user = coreIdentity.Users.FirstOrDefault(u => u.UserName == userName);

            return user != null && Task.Run(() => identity.IsInRoleAsync(user.Id, roleName)).Result;
        }

        public void Dispose()
        { 
            identity?.Dispose();
            coreIdentity?.Dispose();
        }

    }
}