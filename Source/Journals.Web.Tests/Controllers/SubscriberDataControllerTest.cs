using System;
using FluentAssertions;
using FluentAssertions.Mvc;
using Journals.Model;
using Journals.Repository;
using Journals.Web.Controllers;
using Journals.Web.Tests.TestData;
using Xunit;

namespace Journals.Web.Tests.Controllers
{
    public class SubscriberDataControllerTest :
        DataControllerTest<SubscriberController, Subscription, ISubscriptionRepository, SubscriptionTestData>
    {

        protected override SubscriberController CreateControllerInstance(
            ISubscriptionRepository repository,
            IStaticMembershipService membershipService)
        {
            var journalData = new JournalTestData();
            var journalRepo = journalData.CreateRepository();

            return new SubscriberController(journalRepo, repository);
        }

        [Theory]
        [InlineData(2)]
        public void Index_Has_All_Journals(int count)
        {
            var controller = GetController();

            var result = controller.Index();

            result.Should().BeViewResult().WithDefaultViewName();
        }
    }
}