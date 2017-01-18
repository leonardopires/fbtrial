using System;
using Autofac;
using Journals.Model;
using Journals.Repository;
using Journals.Services;
using Journals.Web.Tests.Framework;
using LP.Test.Framework.Core;
using Microsoft.Extensions.Logging;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Journals.Web.Tests.Framework
{
}

namespace Journals.Web.Tests.TestData
{
    public class MocksModule : InitializationModule
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
                           var testData = c.Resolve<JournalTestData>();

                           var membershipRepository = Mock.Create<IStaticMembershipService>();
                           var userMock = new ApplicationUser { Id = testData.DefaultUserId, UserName = "user1"};

                           membershipRepository.Arrange(m => m.GetUser()).Returns(userMock);
                           membershipRepository.Arrange(m => m.GetUser(Arg.Is(testData.DefaultUserId))).Returns(
                                                   (string id) => new ApplicationUser() {Id = id, UserName = "user1"});

                           return membershipRepository;

                       }).As<IStaticMembershipService>().InstancePerLifetimeScope();

            builder.Register(
                       c =>
                       {
                           var repo = new MockJournalRepository(c.Resolve<ITestData<Journal>>());
                           repo.ArrangeMock();

                           return repo;
                       }).As<IJournalRepository>().InstancePerLifetimeScope();

            builder.Register(
                c =>
                {
                    var repo = new MockSubscriptionRepository(c.Resolve<IJournalRepository>(), c.Resolve<IStaticMembershipService>(), c.Resolve<ITestData<Subscription>>());
                    repo.ArrangeMock();

                    return repo;
                }
                ).As<ISubscriptionRepository>().InstancePerLifetimeScope();
        }

    }
}