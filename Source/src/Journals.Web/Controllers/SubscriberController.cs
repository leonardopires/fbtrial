using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Journals.Model;
using Journals.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Journals.Web.Controllers
{
    [Authorize]
    public class SubscriberController : Controller
    {

        private readonly IStaticMembershipService membership;

        private IJournalRepository _journalRepository;

        private readonly ISubscriptionRepository _subscriptionRepository;

        private IMapper Mapper { get; }

        public SubscriberController(
            IJournalRepository journalRepo,
            ISubscriptionRepository subscriptionRepo,
            IStaticMembershipService membership, 
            IMapper mapper)
        {
            _journalRepository = journalRepo;
            _subscriptionRepository = subscriptionRepo;
            this.membership = membership;
            Mapper = mapper;
        }

        public IActionResult Index()
        {
            IActionResult result;
            var journals = _subscriptionRepository.GetAllJournals();

            if (journals != null)
            {
                var userId = GetUserId();
                var subscriptions = _subscriptionRepository.GetJournalsForSubscriberByUserName(userId);

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

        private string GetUserId()
        {
            return membership.GetUser().Id;
        }

        public IActionResult Subscribe(int journalId)
        {
            return RedirectOnSuccess(() => _subscriptionRepository.AddSubscription(journalId, GetUserId()));
        }

        public IActionResult UnSubscribe(int journalId)
        {
            return RedirectOnSuccess(() => _subscriptionRepository.UnSubscribe(journalId, GetUserId()));
        }

        protected IActionResult RedirectOnSuccess(Func<OperationStatus> operation, string actionName = "Index")
        {
            IActionResult result;

            var opStatus = operation?.Invoke();

            if (!opStatus.Status)
            {
                result = NotFound();
            }
            else
            {
                result = RedirectToAction(actionName);
            }
            return result;
        }

    }
}
