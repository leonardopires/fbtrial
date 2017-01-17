using System;
using System.Linq;
using Autofac;
using AutoMapper;
using Journals.Model;
using Journals.Repository;
using Journals.Repository.DataContext;
using Journals.Web.Data;
using Journals.Web.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Classic = Microsoft.AspNet.Identity;
using Compat = Microsoft.AspNet.Identity.CoreCompat;
using Core = Microsoft.AspNetCore.Identity;

namespace Journals.Web
{
    public class JournalsWebModule : Module
    {

        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <param name="builder">The builder through which components can be
        /// registered.</param>
        /// <remarks>Note that the ContainerBuilder parameter is unique to this module.</remarks>
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);
             
            builder.Register(
                r =>
                {
                    MapperConfiguration config = new MapperConfiguration(
                        c =>
                        {
                            c.CreateMap<Journal, JournalViewModel>();
                            c.CreateMap<JournalViewModel, Journal>();

                            c.CreateMap<Journal, JournalUpdateViewModel>();
                            c.CreateMap<JournalUpdateViewModel, Journal>();

                            c.CreateMap<Journal, SubscriptionViewModel>();
                            c.CreateMap<SubscriptionViewModel, Journal>();
                        });
                    return config.CreateMapper();
                }).As<IMapper>().SingleInstance();

            builder.Register(c => 
                new ApplicationIdentityContext(
                        c.Resolve<IConfiguration>().GetConnectionString("DefaultConnection")
                    )
                )
            .AsSelf()
            .As<Compat.IdentityDbContext<ApplicationUser>>();

            builder.Register(c =>
                new JournalsContext(
                        c.Resolve<IConfiguration>().GetConnectionString("DefaultConnection")
                    )
                )
            .AsSelf();


            builder.RegisterType<JournalRepository>().As<IJournalRepository>();
            builder.RegisterType<SubscriptionRepository>().As<ISubscriptionRepository>();

            builder.RegisterType<Core.EntityFrameworkCore.UserStore<ApplicationUserCore>>().AsSelf().As<Core.IUserStore<ApplicationUserCore>>();
            builder.RegisterType<Core.UserManager<ApplicationUserCore>>().AsSelf();


            builder.Register(
                c => new Compat.UserStore<ApplicationUser>(c.Resolve<Compat.IdentityDbContext<ApplicationUser>>())
                ).AsSelf().As<Classic.IUserStore<ApplicationUser>>();

            builder.Register(
                c=> new Classic.UserManager<ApplicationUser>(c.Resolve<Classic.IUserStore<ApplicationUser>>())
                ).AsSelf();


            builder.RegisterType<StaticMembershipService>().As<IStaticMembershipService>();


            builder.Register(c => Log.Logger).As<Serilog.ILogger>();

            builder.RegisterType<ApplicationDbContext>()
                .AsSelf()
                .As<Core.EntityFrameworkCore.IdentityDbContext<ApplicationUserCore, ApplicationRoleCore, string>>()
            ;

            var seeders = AppDomain.CurrentDomain.GetAssemblies()
                                      .SelectMany(a => a.GetTypes())
                                      .Where(
                                          t => t.IsClass &&
                                          t.GetInterfaces()
                                                .Any(i => i == typeof(IDbSeeder))
                                      ).ToArray();

            foreach (var seeder in seeders)
            {
                builder.RegisterType(seeder).As<IDbSeeder>();
            }

            // Add application services.
            builder.RegisterType<AuthMessageSender>().As<IEmailSender>().As<ISmsSender>();
        }

    }
}