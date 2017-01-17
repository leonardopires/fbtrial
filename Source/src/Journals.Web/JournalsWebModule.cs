using System;
using System.Linq;
using Autofac;
using AutoMapper;
using Journals.Model;
using Journals.Repository;
using Journals.Repository.DataContext;
using Journals.Web.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Serilog;

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



            builder.RegisterType<JournalRepository>().As<IJournalRepository>();
            builder.RegisterType<SubscriptionRepository>().As<ISubscriptionRepository>();

            builder.RegisterType<StaticMembershipService>().As<IStaticMembershipService>();

            //builder.RegisterType<JournalsContext>().AsSelf();

            builder.Register(c => Log.Logger).As<Serilog.ILogger>();

            builder.RegisterType<ApplicationDbContext>()
                   .AsSelf()
                   .As<IdentityDbContext<ApplicationUser, ApplicationRole, string>>()
                   .InstancePerLifetimeScope();
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