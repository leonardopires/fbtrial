using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Journals.Model;
using LP.Framework.Data;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Journals.Data
{
   
    public class IdentitySeeder :  IDbSeeder
    {

        private readonly ILogger logger;
        private readonly UserManager<ApplicationUser> userManager;
        private readonly RoleManager<ApplicationRole> roleManager;
        private readonly Func<JournalsContext> journalsContextFactory;

        public IdentitySeeder(
            ILogger<IdentitySeeder> logger,
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            Func<JournalsContext> journalsContextFactory
            )
        {
            this.userManager = userManager;
            this.logger = logger;
            this.roleManager = roleManager;
            this.journalsContextFactory = journalsContextFactory;
        }

        public async Task Seed()
        {
            using (var context = journalsContextFactory())
            {
                context.Database.EnsureCreated();
            }

            await SeedIdentity();
            await SeedJournals();
        }


        private async Task SeedJournals()
        {
            using (var journalsContext = journalsContextFactory())
            {
                var user = await userManager.FindByNameAsync("pappu");

                if (await journalsContext.Journals.CountAsync() == 0)
                {
                    for (int i = 0; i < 10; i++)
                    {
                        var result = journalsContext.Journals.Add(
                            new Journal()
                            {
                                Title = $"Journal {i}",
                                Description = $"Description {i}",
                                UserId = user.Id
                            }                    
                        );
                        result.State = EntityState.Added;
                    }
                }
                await journalsContext.SaveChangesAsync();
            }
        }

        private async Task SeedIdentity()
        {
            var publisher = "Publisher";
            var subscriber = "Subscriber";
            var defaultPassword = "Passw0rd!";

            logger.LogInformation($">> Creating roles.");

            await CreateRole(publisher);
            await CreateRole(subscriber);

            logger.LogInformation(">> Creating users");

            await CreateUser("pappu", defaultPassword, publisher);
            await CreateUser("pappy", defaultPassword, subscriber);
            await CreateUser("daniel", defaultPassword, publisher);
            await CreateUser("andrew", defaultPassword, subscriber);
            await CreateUser("serge", defaultPassword, subscriber);
            await CreateUser("harold", defaultPassword, publisher);

            logger.LogInformation(">> Adding users to roles");

            await AddUserToRole("pappu", publisher);
            await AddUserToRole("pappy", subscriber);
            await AddUserToRole("daniel", publisher);
            await AddUserToRole("andrew", subscriber);
            await AddUserToRole("serge", subscriber);

            await AddUserToRole("harold", publisher);
            await CreateRole(publisher);
            await CreateRole(subscriber);

            logger.LogInformation(">> Creating users");

            await CreateUser("pappu", defaultPassword, publisher);
            await CreateUser("pappy", defaultPassword, subscriber);
            await CreateUser("daniel", defaultPassword, publisher);
            await CreateUser("andrew", defaultPassword, subscriber);
            await CreateUser("serge", defaultPassword, subscriber);
            await CreateUser("harold", defaultPassword, publisher);

            logger.LogInformation(">> Adding users to roles");

            await AddUserToRole("pappu", publisher);
            await AddUserToRole("pappy", subscriber);
            await AddUserToRole("daniel", publisher);
            await AddUserToRole("andrew", subscriber);
            await AddUserToRole("serge", subscriber);

            await AddUserToRole("harold", publisher);
        }

        private async Task CreateRole(string roleName)
        {
            try
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
            catch (Exception ex)
            {
                logger.LogCritical($"Unhandled exception creating role {roleName}.\n\n{ex}\n");
            }
        }

        private async Task CreateUser(string userName, string defaultPassword, string roleName)
        {
            try
            {
                if (await userManager.FindByNameAsync(userName) == null)
                {

                    var user = new ApplicationUser
                    {
                        UserName = userName,
                        Email = $"me+{userName}@leonardopires.net",                                                
                    };


                    logger.LogDebug(
                        $"Creating test user created: {userName}, {defaultPassword}, {roleName}\n");

                    var result = await userManager.CreateAsync(user, defaultPassword);

                    if (result.Succeeded)
                    {
                        logger.LogDebug($"Test user created: {userName}, {defaultPassword}, {roleName}\n");
                    }
                    else
                    {
                        logger.LogError($"Failed to create user: {string.Join((string) ", ", (IEnumerable<string>) result.Errors.Select(e => e.Description))}\n");
                    }
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
                var createdUser = await userManager.FindByNameAsync(userName);

                if (createdUser != null && !await userManager.IsInRoleAsync(createdUser, roleName))
                {

                    await userManager.AddToRoleAsync(createdUser, roleName);
                }
            }
            catch (Exception ex)
            {
                logger.LogCritical($"Unhandled exception creating user {userName}.\n\n{ex}\n");
            }
        }

        public void Dispose()
        {
            //context?.Dispose();
        }

    }
}