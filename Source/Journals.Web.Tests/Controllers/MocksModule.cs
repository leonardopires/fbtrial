using System.Linq;
using System.Web.Security;
using Autofac;
using Journals.Model;
using Journals.Repository;
using Journals.Web.Tests.TestData;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Journals.Web.Tests.Controllers
{
    public class MocksModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            base.Load(builder);

            builder.RegisterType<JournalTestData>().AsSelf().As<ITestData<Journal>>();
            builder.RegisterType<SubscriptionTestData>().AsSelf().As<ITestData<Subscription>>();
            builder.RegisterType<StaticPagesTestData>().AsSelf().As<ITestData<object>>();

            builder.Register(
                       c =>
                       {
                           var membershipRepository = Mock.Create<IStaticMembershipService>();
                           var userMock = Mock.Create<MembershipUser>();

                           userMock.Arrange(u => u.UserName).Returns("user1");
                           userMock.Arrange(u => u.ProviderUserKey).Returns(1);

                           membershipRepository.Arrange(m => m.GetUser()).Returns(userMock);
                           membershipRepository.Arrange(m => m.GetUserProfile(Arg.Is(1))).Returns(
                                                   (int id) => new UserProfile() {UserId = id, UserName = "user1"});

                           return membershipRepository;

                       }).As<IStaticMembershipService>().InstancePerLifetimeScope();

            builder.RegisterType<MockJournalRepository>().As<IJournalRepository>().InstancePerLifetimeScope();
            builder.RegisterType<MockSubscriptionRepository>().As<ISubscriptionRepository>().InstancePerLifetimeScope();
        }

    }
}