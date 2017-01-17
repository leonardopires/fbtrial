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
            try
            {
                using (DataContext)
                {
                    var result = DataContext.Journals.Include("Subscription").Where(a => a.Title != null);


                    var list = result.AsEnumerable()
                                     .Select(
                                         f => new Journal
                                         {
                                             Id = f.Id,
                                             Title = f.Title,
                                             Description = f.Description,
                                             UserId = f.UserId
                                         }).ToList();

                    return list;
                }
            }
            catch (Exception e)
            {
                OperationStatus.CreateFromException("Error fetching subscriptions: ", e);
                ;
            }

            return new List<Journal>();
        }

        public List<Subscription> GetJournalsForSubscriber(string userId)
        {
            try
            {
                using (DataContext)
                {
                    var subscriptions = DataContext.Subscriptions.Include("Journal").Where(u => u.UserId == userId);
                    if (subscriptions != null)
                    {
                        return subscriptions.ToList();
                    }
                }
            }
            catch (Exception e)
            {
                OperationStatus.CreateFromException("Error fetching subscriptions: ", e);
                ;
            }

            return new List<Subscription>();
        }

        public List<Subscription> GetJournalsForSubscriberByUserName(string userName)
        {
            try
            {
                using (DataContext)
                {
                    var subscriptions =
                        DataContext.Subscriptions.Include("Journal").Where(u => u.User.UserName == userName);

                    if (subscriptions != null)
                    {
                        return subscriptions.ToList();
                    }
                }
            }
            catch (Exception e)
            {
                OperationStatus.CreateFromException("Error fetching subscriptions: ", e);
                ;
            }

            return new List<Subscription>();
        }

        public OperationStatus AddSubscription(int journalId, string userId)
        {
            var opStatus = new OperationStatus {Status = true};
            try
            {
                using (DataContext)
                {
                    var s = new Subscription
                    {
                        JournalId = journalId,
                        UserId = userId
                    };

                    var j = DataContext.Subscriptions.Add(s);
                    DataContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                opStatus = OperationStatus.CreateFromException("Error adding subscription: ", e);
            }

            return opStatus;
        }

        public OperationStatus UnSubscribe(int journalId, string userId)
        {
            var opStatus = new OperationStatus {Status = true};
            try
            {
                using (DataContext)
                {
                    var subscriptions =
                        DataContext.Subscriptions.Where(u => u.JournalId == journalId && u.UserId == userId);

                    foreach (var s in subscriptions)
                    {
                        DataContext.Subscriptions.Remove(s);
                    }
                    DataContext.SaveChanges();
                }
            }
            catch (Exception e)
            {
                opStatus = OperationStatus.CreateFromException("Error deleting subscription: ", e);
            }

            return opStatus;
        }

    }
}
