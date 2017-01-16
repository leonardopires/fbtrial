using System;
using Journals.Model;
using System.Collections.Generic;

namespace Journals.Repository
{
    public interface ISubscriptionRepository : IDisposable
    {
        List<Journal> GetAllJournals();

        OperationStatus AddSubscription(int journalId, int userId);

        List<Subscription> GetJournalsForSubscriber(int userId);

        OperationStatus UnSubscribe(int journalId, int userId);

        List<Subscription> GetJournalsForSubscriber(string userName);
    }
}