﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.AccessControl;
using System.Threading.Tasks;
using AutoMapper;
using Journals.Model;
using Journals.Repository;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
using Microsoft.Extensions.Logging;

namespace Journals.Web.Controllers
{
    [Authorize]
    public class PublisherController : Controller
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

        public IActionResult GetFile(int Id)
        {
            var j = _journalRepository.GetJournalById(Id);

            if (j == null)
            {
                return NotFound();
            }
            return File(j.Content, j.ContentType);
        }

        [HttpPost("upload")]
        public async Task<IActionResult> PostUpload(IFormFile file)
        {
            try
            {
                using (var stream = file.OpenReadStream())
                {
                    var directory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Uploads"));
                    directory.Create();

                    var localFile = new FileInfo(Path.Combine(directory.ToString(), $"{DateTime.UtcNow:yyyyMMdd-hhmmss}_{file.FileName}"));

                    using (var fileStream = localFile.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.Delete))
                    {
                        await stream.CopyToAsync(fileStream);

                        Logger.LogDebug($"Saved file {localFile.FullName}");

                        return Ok(new OperationStatus() { Status = true });
                    }
                }
            }
            catch (IOException ex)
            {
                var status =
                    OperationStatus.CreateFromException("An error occurred while saving the uploaded file.", ex);

                return StatusCode(500, status);
            }

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

        public IActionResult Delete(int journalId)
        {
            IActionResult result;
            var selectedJournal = _journalRepository.GetJournalById(journalId);

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

        public IActionResult Edit(int journalId)
        {
            IActionResult result;

            var selectedJournal = _journalRepository.GetJournalById(journalId);


            if (selectedJournal != null)
            {
                var journal = Mapper.Map<Journal, JournalUpdateViewModel>(selectedJournal);
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

                //JournalHelper.PopulateFile(journal.File, selectedJournal);

                var opStatus = _journalRepository.UpdateJournal(selectedJournal);

                if (!opStatus.Status)
                {
                    result = StatusCode((int)HttpStatusCode.InternalServerError);
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

