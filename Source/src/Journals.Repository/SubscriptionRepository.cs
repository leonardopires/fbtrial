using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using Journals.Model;
using Journals.Repository.DataContext;

namespace Journals.Repository
{
    public class SubscriptionRepository : RepositoryBase<JournalsContext>, ISubscriptionRepository
    {

        public SubscriptionRepository(Func<JournalsContext> contextFactory) : base(contextFactory)
        {
        }

        public List<Journal> GetAllJournals()
        {
            using (DataContext)
            {
                return Table<Subscription>().Include("Subscription").Select(s => s.Journal).Where(j => j.Title != null)?.ToList();
            }
        }

        public List<Subscription> GetJournalsForSubscriber(string userId)
        {
            using (DataContext)
            {                
                return Table<Subscription>().Include("Journal").Where(u => u.UserId == userId)?.ToList();
            }
        }

        public List<Subscription> GetJournalsForSubscriberByUserName(string userName)
        {
            using (DataContext)
            {
                return Table<Subscription>().Include("Journal").Where(u => u.User.UserName == userName)?.ToList();
            }
        }

        public OperationStatus AddSubscription(int journalId, string userId)
        {
            var subscription = new Subscription
            {
                JournalId = journalId,
                UserId = userId
            };

            return ExecuteOperations(
                Add(subscription),
                Save
                );
        }

        public OperationStatus UnSubscribe(int journalId, string userId)
        {
            IEnumerable<Subscription> subscriptions;

            return ExecuteOperations(
                context =>
                {
                    subscriptions = GetMany<Subscription>(u => u.JournalId == journalId && u.UserId == userId);

                    foreach (var subscription in subscriptions)
                    {
                        context.Data.Subscriptions.Remove(subscription);
                    }
                },
                Save                
            );
        }

    }
}
