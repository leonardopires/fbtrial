using System.Collections.Generic;
using System.Linq;
using Journals.Model;
using Journals.Repository;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Journals.Web.Tests.TestData
{
    public class MockSubscriptionRepository : ISubscriptionRepository
    {
        private readonly List<Subscription> models;
        private ISubscriptionRepository mock;

        public MockSubscriptionRepository(
            IJournalRepository journalRepository,
            IStaticMembershipService membershipRepository,
            ITestData<Subscription> testData)
        {
            this.models = new List<Subscription>(testData.GetDefaultData());

            mock = Mock.Create<ISubscriptionRepository>();


            ArrangeMock(journalRepository, membershipRepository);
        }

        private void ArrangeMock(IJournalRepository journalRepository, IStaticMembershipService membershipRepository)
        {
            mock.Arrange((r) => r.GetAllJournals()).Returns(models.Select(m => m.Journal).ToList());

            mock.Arrange((r) => r.GetJournalsForSubscriber(Arg.IsAny<int>())).Returns(
                    (int id) =>
                    {
                        var subscriptions = models.Where(i => i.UserId == id).ToList();
                        return subscriptions;
                    });

            mock.Arrange(r => r.AddSubscription(Arg.IsAny<int>(), Arg.IsAny<int>())).Returns(
                    (int journalId, int userId) =>
                    {
                        var journal = journalRepository.GetJournalById(journalId);
                        var user = membershipRepository.GetUserProfile(userId);

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

            mock.Arrange(r => r.UnSubscribe(Arg.IsAny<int>(), Arg.IsAny<int>())).Returns(
                    (int id, int userId) =>
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
        }


        public void Dispose()
        {
            mock.Dispose();
        }

        public List<Journal> GetAllJournals()
        {
            return mock.GetAllJournals();
        }

        public OperationStatus AddSubscription(int journalId, int userId)
        {
            return mock.AddSubscription(journalId, userId);
        }

        public List<Subscription> GetJournalsForSubscriber(int userId)
        {
            return mock.GetJournalsForSubscriber(userId);
        }

        public OperationStatus UnSubscribe(int journalId, int userId)
        {
            return mock.UnSubscribe(journalId, userId);
        }

        public List<Subscription> GetJournalsForSubscriber(string userName)
        {
            return mock.GetJournalsForSubscriber(userName);
        }

    }
}