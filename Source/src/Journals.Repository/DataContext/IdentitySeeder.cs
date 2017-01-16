using System;
using System.Linq;
using System.Threading.Tasks;
using Journals.Model;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.EntityFramework;
using Microsoft.Extensions.Logging;

using Compat = Microsoft.AspNet.Identity.CoreCompat;
using Classic = Microsoft.AspNet.Identity;

namespace Journals.Repository.DataContext
{
    public class IdentitySeeder : IDbSeeder
    {

        private readonly Func<Compat.IdentityDbContext<ApplicationUser>> identityContextFactory;
        private readonly ILogger logger;

        public IdentitySeeder(Func<Compat.IdentityDbContext<ApplicationUser>> identityContextFactory, ILogger<IdentitySeeder> logger)
        {
            this.identityContextFactory = identityContextFactory;
            this.logger = logger;
        }

        public async Task Seed()
        {

            var publisher = "Publisher";
            var subscriber = "Subscriber";
            var defaultPassword = "Passw0rd";


            await Task.WhenAll(
                CreateRole(publisher),
                CreateRole(subscriber)
            );

            await Task.WhenAll(
                CreateUser("pappu", defaultPassword, publisher),
                CreateUser("pappy", defaultPassword, subscriber),
                CreateUser("daniel", defaultPassword, publisher),
                CreateUser("andrew", defaultPassword, subscriber),
                CreateUser("serge", defaultPassword, subscriber),
                CreateUser("harold", defaultPassword, publisher)
            );

        }

        private async Task CreateUser(string userName, string defaultPassword, string roleName)
        {
            try
            {
                using (var identity = identityContextFactory())
                {
                    using (var store = new Compat.UserStore<ApplicationUser>(identity))
                    {
                        using (var manager = new UserManager<ApplicationUser>(store))
                        {

                            if (await manager.FindByNameAsync(userName) == null)
                            {
                                var user = new ApplicationUser();

                                user.UserName = userName;
                                user.Email = $"me+{userName}@leonardopires.net";

                                var result = await manager.CreateAsync(user, defaultPassword);
                                if (result.Succeeded)
                                {
                                    var createdUser = await manager.FindByNameAsync(userName);
                                    await manager.AddToRoleAsync(createdUser.Id, roleName);
                                    logger.LogDebug(null, $"Test user created: {userName}, {defaultPassword}, {roleName}");
                                }
                                else
                                {
                                    logger.LogError(null, "Failed to create user {@errors}", result.Errors);
                                }
                            }
                        }
                    }
                    await identity.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(null, ex, "Error: ", ex);
            }
        }

        private async Task CreateRole(string roleName)
        {
            try
            {
                using (var identity = identityContextFactory())
                {
                    using (var roleStore = new RoleStore<ApplicationRole, string, IdentityUserRole>(identity))
                    {
                        using (var roleManager = new RoleManager<ApplicationRole, string>(roleStore))
                        {
                            if (await roleManager.FindByNameAsync(roleName) == null)
                            {
                                var role = new ApplicationRole() {Name = roleName};
                                var result = await roleManager.CreateAsync(role);

                                if (!result.Succeeded)
                                {
                                    logger.LogError(null, "Failed to create role {@errors}", result.Errors);
                                }
                            }
                        }
                    }
                    await identity.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical(null, ex, "Error: ", ex);
            }
        }
    }
}