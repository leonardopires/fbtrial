using Autofac;
using AutoMapper;
using Journals.Model;
using Journals.Web.Services;

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


            // Add application services.
            builder.RegisterType<AuthMessageSender>().As<IEmailSender>().As<ISmsSender>();
        }

    }
}