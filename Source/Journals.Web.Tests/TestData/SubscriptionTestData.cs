using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Security;
using Journals.Model;
using Journals.Repository;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Journals.Web.Tests.TestData
{
    public class SubscriptionTestData : RepositoryTestData<Subscription, ISubscriptionRepository>
    {
        public override void SetUpRepository(List<Subscription> models, ISubscriptionRepository modelRepository)
        {
            var membershipRepository = Mock.Create<IStaticMembershipService>();
            var userMock = Mock.Create<MembershipUser>();

            userMock.Arrange(u => u.ProviderUserKey).Returns(1);
            membershipRepository.Arrange(m => m.GetUser()).Returns(userMock);

            modelRepository.Arrange((r) => r.GetAllJournals()).Returns(models);

            foreach (var journal in models)
            {
                modelRepository.Arrange((r) => r.GetJournalsForSubscriber(userMock.UserName)).Returns(journal);
            }
            
        }

    }
}