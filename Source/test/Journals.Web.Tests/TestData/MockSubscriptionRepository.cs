using System.Collections.Generic;
using System.Linq;
using Journals.Model;
using Journals.Repository;
using LP.Test.Framework.Core;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Journals.Web.Tests.TestData
{
    public class MockSubscriptionRepository : MockRepositoryWrapper<Subscription, ISubscriptionRepository>, ISubscriptionRepository
    {

        private readonly IJournalRepository journalRepository;
        private readonly IStaticMembershipService membershipRepository;

        public MockSubscriptionRepository(
            IJournalRepository journalRepository,
            IStaticMembershipService membershipRepository,
            ITestData<Subscription> testData) : base(testData)
        {
            this.journalRepository = journalRepository;
            this.membershipRepository = membershipRepository;
        }

        public override void ArrangeMock()
        {
            mock.Arrange((r) => r.GetAllJournals()).Returns(models.Select(m => m.Journal).ToList());

            mock.Arrange((r) => r.GetJournalsForSubscriber(Arg.IsAny<string>())).Returns(
                    (string id) =>
                    {
                        var subscriptions = models.Where(i => i.UserId == id).ToList();
                        return subscriptions;
                    });

            mock.Arrange(r => r.AddSubscription(Arg.IsAny<int>(), Arg.IsAny<string>())).Returns(
                    (int journalId, string userId) =>
                    {
                        var journal = journalRepository.GetJournalById(journalId);
                        var user = membershipRepository.GetUser(userId);

                        if (user != null
                            && journal != null)
                        {
                            var sub = new Subscription()
                            {
                                Id = models.Count(),
                                Journal = journal,
                                JournalId = journalId,
                                User = user,
                                UserId = userId
                            };
                            models.Add(sub);
                        }
                        return new OperationStatus()
                        {
                            Status = user != null && journal != null && models.Any(i => i.Id == journalId && i.UserId == userId)
                        };
                    });

            mock.Arrange(r => r.UnSubscribe(Arg.IsAny<int>(), Arg.IsAny<string>())).Returns(
                    (int id, string userId) =>
                    {
                        var index = models.FindIndex(i => i.Id == id && i.UserId == userId);
                        if (index >= 0)
                        {
                            models.RemoveAt(index);
                        }
                        return new OperationStatus()
                        {
                            Status = index >= 0 && models.Count(i => i.Id == id && i.UserId == userId) == 0
                        };
                    });


            mock.Arrange((r) => r.GetJournalsForSubscriberByUserName(Arg.IsAny<string>())).Returns(
                    (string username) =>
                    {
                        var subscriptions = models.Where(i => i.User?.UserName == username).ToList();
                        return subscriptions;
                    });
        }


        public List<Journal> GetAllJournals()
        {
            return mock.GetAllJournals();
        }

        public OperationStatus AddSubscription(int journalId, string userId)
        {
            return mock.AddSubscription(journalId, userId);
        }

        public List<Subscription> GetJournalsForSubscriber(string userId)
        {
            return mock.GetJournalsForSubscriber(userId);
        }

        public OperationStatus UnSubscribe(int journalId, string userId)
        {
            return mock.UnSubscribe(journalId, userId);
        }

        public List<Subscription> GetJournalsForSubscriberByUserName(string userName)
        {
            return mock.GetJournalsForSubscriber(userName);
        }
    }
}