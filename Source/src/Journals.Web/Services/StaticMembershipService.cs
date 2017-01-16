using System.IdentityModel.Claims;
using System.Linq;
using System.Threading.Tasks;
using Journals.Model;
using Journals.Web.Data;
using Journals.Web.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

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

            return identity.Users.FirstOrDefault(i => i.Id == identity.GetUserId(user));
        }

        public ApplicationUser GetUser(string userId)
        {
            return Task.Run(() => identity.FindByIdAsync(userId)).Result;
        }
        public bool IsUserInRole(string userName, string roleName)
        {

            var user = identity.Users.FirstOrDefault(u => u.UserName == userName);

            return Task.Run(() => identity.IsInRoleAsync(user, roleName)).Result;
        }
    }
}