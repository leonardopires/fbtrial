using System;
using Journals.Model;
using System.Collections.Generic;

namespace Journals.Repository
{
    public interface ISubscriptionRepository : IDisposable
    {
        List<Journal> GetAllJournals();

        OperationStatus AddSubscription(int journalId, string userId);

        List<Subscription> GetJournalsForSubscriber(string userId);

        OperationStatus UnSubscribe(int journalId, string userId);

        List<Subscription> GetJournalsForSubscriberByUserName(string userName);
    }
}