using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Journals.Model;
using Journals.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;

namespace Journals.Web.Controllers
{
    [Authorize]
    [FormatFilter]
    public class SubscriberController : JournalControllerBase
    {

        private readonly IStaticMembershipService membership;

        private readonly ISubscriptionRepository _subscriptionRepository;

        private IMapper Mapper { get; }

        public SubscriberController(
            ISubscriptionRepository subscriptionRepo,
            IStaticMembershipService membership, 
            IMapper mapper)
        {
            _subscriptionRepository = subscriptionRepo;
            this.membership = membership;
            Mapper = mapper;
        }

        public IActionResult Index(string format=null)
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
                result = Result(format, subscriberModel);
            }

            else
            {
                result = Result(format, new List<SubscriptionViewModel>());
            }
            return result;
        }

        private string GetUserId()
        {
            return membership.GetUser()?.Id;
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

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _subscriptionRepository.Dispose();
                membership.Dispose();
            }
            base.Dispose(disposing);
        }

    }
}
