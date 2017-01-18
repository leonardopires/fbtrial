using System;
using System.Collections.Generic;
using Journals.Model;

namespace Journals.Services
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