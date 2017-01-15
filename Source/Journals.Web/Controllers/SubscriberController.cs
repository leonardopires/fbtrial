using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Mvc;
using AutoMapper;
using Journals.Model;
using Journals.Repository;

namespace Journals.Web.Controllers
{
    [Authorize]
    public class SubscriberController : Controller
    {

        private readonly IStaticMembershipService membership;
        private IJournalRepository _journalRepository;
        private readonly ISubscriptionRepository _subscriptionRepository;

        public SubscriberController(
            IJournalRepository journalRepo,
            ISubscriptionRepository subscriptionRepo,
            IStaticMembershipService membership)
        {
            _journalRepository = journalRepo;
            _subscriptionRepository = subscriptionRepo;
            this.membership = membership;
        }

        public ActionResult Index()
        {
            ActionResult result;
            var journals = _subscriptionRepository.GetAllJournals();

            if (journals != null)
            {
                var userId = GetUserId();
                var subscriptions = _subscriptionRepository.GetJournalsForSubscriber(userId);

                var subscriberModel = Mapper.Map<List<Journal>, List<SubscriptionViewModel>>(journals);

                foreach (var journal in subscriberModel)
                {
                    if (subscriptions.Any(k => k.JournalId == journal.Id))
                    {
                        journal.IsSubscribed = true;
                    }
                }

                result = View(subscriberModel);
            }

            else
            {
                result = View(new List<SubscriptionViewModel>());
            }
            return result;
        }

        private int GetUserId()
        {
            return (int) (membership?.GetUser()?.ProviderUserKey ?? -1);
        }

        public ActionResult Subscribe(int Id)
        {
            return RedirectOnSuccess(() => _subscriptionRepository.AddSubscription(Id, GetUserId()));
        }

        public ActionResult UnSubscribe(int Id)
        {
            return RedirectOnSuccess(() => _subscriptionRepository.UnSubscribe(Id, GetUserId()));
        }

        protected ActionResult RedirectOnSuccess(Func<OperationStatus> operation, string actionName = "Index")
        {
            ActionResult result;

            var opStatus = operation?.Invoke();

            if (!opStatus.Status)
            {
                result = HttpNotFound();
            }
            else
            {
                result = RedirectToAction(actionName);
            }
            return result;
        }

    }
}
