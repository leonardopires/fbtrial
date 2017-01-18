using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.AccessControl;
using System.Threading.Tasks;
using AutoMapper;
using Journals.Model;
using Journals.Repository;
using Journals.Web.Helpers;
using Journals.Web.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Serilog;
using ILogger = Microsoft.Extensions.Logging.ILogger;

namespace Journals.Web.Controllers
{
    [Authorize]
    public class PublisherController : JournalControllerBase
    {

        private readonly IJournalRepository _journalRepository;
        private readonly IStaticMembershipService _membershipService;

        private ILogger Logger { get; }

        private IMapper Mapper { get; }


        public PublisherController(IJournalRepository journalRepo, IStaticMembershipService membershipService, ILogger<PublisherController> logger, IMapper mapper)
        {
            _journalRepository = journalRepo;
            _membershipService = membershipService;
            Logger = logger;
            Mapper = mapper;
        }

        public IActionResult Index()
        {
            var userId = _membershipService.GetUser().Id;

            var allJournals = _journalRepository.GetAllJournals(userId);
            var journals = Mapper.Map<List<Journal>, List<JournalViewModel>>(allJournals);
            return View(nameof(Index), journals);
        }

        public IActionResult Create()
        {
            return View(nameof(Create));
        }

        /// <summary>
        /// Gets the file identified by the specified ID.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>
        ///   <see cref="Microsoft.AspNetCore.Mvc.IActionResult" />
        /// </returns>
        public IActionResult GetFile(int id)
        {
            IActionResult result = NotFound();

            var file = _journalRepository.GetFile(id);                        

            if (file != null)
            {
                result = File(file.Content, file.ContentType, file.FileName);
            }
            return result;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> PostUpload(IFormFile file)
        {
            IActionResult result;
            try
            {
                using (var stream = file.OpenReadStream())
                {

                    using (var binaryReader = new BinaryReader(stream))
                    {
                        var dbFile = new Model.File()
                        {
                            // dirty quick fix that would work since max file size is small
                            Content = binaryReader.ReadBytes((int)file.Length),
                            ContentType = file.ContentType,
                            FileName = file.FileName,
                            Length = file.Length,
                            ModifiedDate = DateTime.UtcNow
                        };


                        _journalRepository.AddFile(dbFile);

                        Logger.LogDebug($"Saved file {file.FileName}");

                        result = Ok(new OperationStatus() { Status = true });
                    }
                    
                }
            }
            catch (IOException ex)
            {
                var status =
                    OperationStatus.CreateFromException("An error occurred while saving the uploaded file.", ex);

                result = StatusCode(500, status);
            }
            return result;

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create(JournalViewModel journal)
        {
            if (ModelState.IsValid)
            {
                var newJournal = Mapper.Map<JournalViewModel, Journal>(journal);

                //JournalHelper.PopulateFile(journal.File, newJournal);

                newJournal.UserId = _membershipService.GetUser().Id;

                var opStatus = _journalRepository.AddJournal(newJournal);
                if (!opStatus.Status)
                {
                    return StatusCode(500);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(nameof(Create), journal);
        }

        public IActionResult Delete(int id)
        {
            IActionResult result;
            var selectedJournal = _journalRepository.GetJournalById(id);

            if (selectedJournal != null)
            {
                var journal = Mapper.Map<Journal, JournalViewModel>(selectedJournal);

                result = View(nameof(Delete), journal);
            }
            else
            {
                result = NotFound();
            }
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Delete(JournalViewModel journal)
        {
            IActionResult result;

            var selectedJournal = Mapper.Map<JournalViewModel, Journal>(journal);
            var opStatus = _journalRepository.DeleteJournal(selectedJournal);

            if (opStatus.Status)
            {
                result = RedirectToAction(nameof(Index));
            }
            else
            {
                result = NotFound();
            }
            return result;
        }

        public IActionResult Edit(int id)
        {
            IActionResult result;

            var selectedJournal = _journalRepository.GetJournalById(id);


            if (selectedJournal != null)
            {
                var journal = Mapper.Map<Journal, JournalUpdateViewModel>(selectedJournal);

                var issues = _journalRepository.GetIssues(journal.Id);

                if (issues != null)
                {
                    foreach (var selectedIssue in issues)
                    {
                        var issue = Mapper.Map<Issue, IssueViewModel>(selectedIssue);

                        journal.Issues.Add(issue);
                    }
                }


                result = View(nameof(Edit), journal);
            }
            else
            {
                result = NotFound();
            }
            return result;
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(JournalUpdateViewModel journal)
        {
            IActionResult result;

            if (ModelState.IsValid)
            {
                var selectedJournal = Mapper.Map<JournalUpdateViewModel, Journal>(journal);

                
                var opStatus = _journalRepository.UpdateJournal(selectedJournal);

                if (!opStatus.Status)
                {
                    ModelState.AddModelError("Unknown", opStatus.Message);
                    result = View(journal).WithStatusCode((int)HttpStatusCode.InternalServerError);
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

        [Route("[controller]/edit/{journalId}/issue")]
        public IActionResult Issue(int journalId)
        {
            return View(new IssueViewModel() {JournalId = journalId});
        }

        [HttpPost]
        [Route("[controller]/edit/{journalId}/issue")]
        public IActionResult Issue(IssueViewModel issue)
        {
            IActionResult result;

            if (ModelState.IsValid)
            {
                var issueToInsert = Mapper.Map<IssueViewModel, Issue>(issue);

                issueToInsert.File = new Model.File();

                issue.File.PopulateFile(issueToInsert.File);

                var status = _journalRepository.AddIssue(issueToInsert);

                if (status.Status)
                {
                    result = RedirectToAction(nameof(Edit), new {id = issue.JournalId});
                }
                else
                {
                    ModelState.AddModelError("Unknown", status.Message ?? status.ExceptionInnerMessage ?? status.ExceptionMessage ?? status.ToString());
                    result = View(issue).WithStatusCode((int)HttpStatusCode.InternalServerError);
                }
            }
            else
            {
                result = View(nameof(Issue), issue).WithStatusCode(HttpStatusCode.BadRequest);
            }

            return result;
        }

    }
}



