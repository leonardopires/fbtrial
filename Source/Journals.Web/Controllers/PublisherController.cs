using AutoMapper;
using Journals.Model;
using Journals.Repository;
using Journals.Web.Filters;
using Journals.Web.Helpers;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web.Mvc;
using Newtonsoft.Json;

namespace Journals.Web.Controllers
{
    [AuthorizeRedirect(Roles = "Publisher")]
    public class PublisherController : Controller
    {
        private IJournalRepository _journalRepository;
        private IStaticMembershipService _membershipService;

        public PublisherController(IJournalRepository journalRepo, IStaticMembershipService membershipService)
        {
            _journalRepository = journalRepo;
            _membershipService = membershipService;
        }

        public ActionResult Index()
        {
            var userId = (int)_membershipService.GetUser().ProviderUserKey;

            List<Journal> allJournals = _journalRepository.GetAllJournals(userId);
            var journals = Mapper.Map<List<Journal>, List<JournalViewModel>>(allJournals);
            return View(nameof(Index), journals);
        }

        public ActionResult Create()
        {
            return View(nameof(Create));
        }

        public ActionResult GetFile(int Id)
        {
            Journal j = _journalRepository.GetJournalById(Id);

            if (j == null)
            {
                return HttpNotFound();
            }
            return File(j.Content, j.ContentType);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(JournalViewModel journal)
        {
            if (ModelState.IsValid)
            {
                var newJournal = Mapper.Map<JournalViewModel, Journal>(journal);
                JournalHelper.PopulateFile(journal.File, newJournal);

                newJournal.UserId = (int)_membershipService.GetUser().ProviderUserKey;

                var opStatus = _journalRepository.AddJournal(newJournal);
                if (!opStatus.Status)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
                return RedirectToAction(nameof(Index));
            }
            else
                return View(nameof(Create), journal);
        }

        public ActionResult Delete(int Id)
        {
            ActionResult result;
            var selectedJournal = _journalRepository.GetJournalById(Id);

            if (selectedJournal != null)
            {

                var journal = Mapper.Map<Journal, JournalViewModel>(selectedJournal);

                result = View(nameof(Delete), journal);
            }
            else
            {
                result = HttpNotFound();
            }
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(JournalViewModel journal)
        {
            ActionResult result;

            var selectedJournal = Mapper.Map<JournalViewModel, Journal>(journal);
            var opStatus = _journalRepository.DeleteJournal(selectedJournal);

            if (opStatus.Status)
            {
                result = RedirectToAction(nameof(Index));
            }
            else
            {
                result = HttpNotFound();
            }
            return result;
        }

        public ActionResult Edit(int Id)
        {
            ActionResult result;

            var selectedJournal = _journalRepository.GetJournalById(Id);


            if (selectedJournal != null)
            {
                var journal = Mapper.Map<Journal, JournalUpdateViewModel>(selectedJournal);
                result = View(nameof(Edit), journal);
            }
            else
            {
                result = HttpNotFound();
            }
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(JournalUpdateViewModel journal)
        {

            ActionResult result;

            if (ModelState.IsValid)
            {
                var selectedJournal = Mapper.Map<JournalUpdateViewModel, Journal>(journal);
                JournalHelper.PopulateFile(journal.File, selectedJournal);

                var opStatus = _journalRepository.UpdateJournal(selectedJournal);

                if (!opStatus.Status)
                {
                    result = new HttpStatusCodeResult(HttpStatusCode.InternalServerError);
                }
                else
                {
                    result = RedirectToAction(nameof(Index));
                }
            }
            else
            {
                result = View(nameof(Edit), journal);
            }
            return result;
        }
    }
}