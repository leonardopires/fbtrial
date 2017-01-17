using System;
using System.Collections.Generic;
using System.Linq;
using Journals.Model;
using Journals.Repository.DataContext;
using Microsoft.EntityFrameworkCore;

namespace Journals.Repository
{
    public class SubscriptionRepository : RepositoryBase<JournalsContext>, ISubscriptionRepository
    {

        private readonly IStaticMembershipService membership;

        public SubscriptionRepository(Func<JournalsContext> contextFactory, IStaticMembershipService membership) : base(contextFactory)
        {
            this.membership = membership;
        }

        public List<Journal> GetAllJournals()
        {
            return Table<Journal>().OrderByDescending(t => t.CreatedDate).ToList();
        }

        public List<Subscription> GetJournalsForSubscriber(string userId)
        {
            return Table<Subscription>().Include(s => s.Journal).Where(u => u.UserId == userId)?.ToList();
        }

        public List<Subscription> GetJournalsForSubscriberByUserName(string userName)
        {
            var user = membership.GetUserByName(userName);
            return GetJournalsForSubscriber(user.Id);
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

        public override void Dispose()
        {
            membership.Dispose();
            base.Dispose();
        }

    }
}
