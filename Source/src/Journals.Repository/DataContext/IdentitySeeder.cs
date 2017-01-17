using System;
using System.Data.Entity;
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
    public class IdentitySeeder :  IDbSeeder, IDatabaseInitializer<Compat.IdentityDbContext<ApplicationUser>>
    {

        private readonly Func<Compat.IdentityDbContext<ApplicationUser>> identityContextFactory;
        private readonly ILogger logger;

        public IdentitySeeder(
            Func<Compat.IdentityDbContext<ApplicationUser>> identityContextFactory,
            ILogger<IdentitySeeder> logger)
        {
            this.identityContextFactory = identityContextFactory;
            this.logger = logger;
        }

        public async Task Seed()
        {

            var publisher = "Publisher";
            var subscriber = "Subscriber";
            var defaultPassword = "Passw0rd";


            logger.LogInformation(">> Creating roles");

            await Task.WhenAll(
                CreateRole(publisher),
                CreateRole(subscriber)
            );

            logger.LogInformation(">> Creating users");

            await Task.WhenAll(
                CreateUser("pappu", defaultPassword, publisher),
                CreateUser("pappy", defaultPassword, subscriber),
                CreateUser("daniel", defaultPassword, publisher),
                CreateUser("andrew", defaultPassword, subscriber),
                CreateUser("serge", defaultPassword, subscriber),
                CreateUser("harold", defaultPassword, publisher)
            );

            logger.LogInformation(">> Adding users to roles");

            await Task.WhenAll(
                AddUserToRole("pappu", publisher),
                AddUserToRole("pappy", subscriber),
                AddUserToRole("daniel", publisher),
                AddUserToRole("andrew", subscriber),
                AddUserToRole("serge", subscriber),
                AddUserToRole("harold", publisher)
            );

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
                                var role = new ApplicationRole()
                                {
                                    Name = roleName,
                                    NormalizedName = roleName.ToUpperInvariant()
                                };
                                var result = await roleManager.CreateAsync(role);

                                if (!result.Succeeded)
                                {
                                    logger.LogError($"Failed to create role {string.Join(", ", result.Errors)}\n");
                                }
                            }
                        }
                    }
                    await identity.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical($"Unhandled exception creating role {roleName}.\n\n{ex}\n");
            }
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

                                var user = new ApplicationUser
                                {
                                    UserName = userName,
                                    Email = $"me+{userName}@leonardopires.net",
                                };


                                logger.LogDebug(
                                    $"Creating test user created: {userName}, {defaultPassword}, {roleName}\n");

                                var result = await manager.CreateAsync(user, defaultPassword);

                                if (result.Succeeded)
                                {
                                    logger.LogDebug($"Test user created: {userName}, {defaultPassword}, {roleName}\n");
                                }
                                else
                                {
                                    logger.LogError($"Failed to create user: {string.Join(", ", result.Errors)}\n");
                                }
                            }
                        }
                    }
                    await identity.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical($"Unhandled exception creating user {userName}.\n\n{ex}\n");
            }
        }

        private async Task AddUserToRole(string userName, string roleName)
        {
            try
            {
                using (var identity = identityContextFactory())
                {
                    using (var store = new Compat.UserStore<ApplicationUser>(identity))
                    {
                        var createdUser = await store.FindByNameAsync(userName);

                        if (!await store.IsInRoleAsync(createdUser, roleName))
                        {

                            await store.AddToRoleAsync(createdUser, roleName);
                        }
                    }
                    await identity.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical($"Unhandled exception creating user {userName}.\n\n{ex}\n");
            }
        }

        public void InitializeDatabase(Compat.IdentityDbContext<ApplicationUser> context)
        {
            
        }

    }
}