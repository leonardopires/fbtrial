using Journals.Model;
using Journals.Repository.DataContext;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.IO;
using System.Linq;
using Microsoft.Extensions.Logging;
using File = Journals.Model.File;

namespace Journals.Repository
{
    public class JournalRepository : RepositoryBase<JournalsContext>, IJournalRepository
    {

        private readonly ILogger<JournalRepository> logger;

        public JournalRepository(Func<JournalsContext> contextFactory, ILogger<JournalRepository> logger) : base(contextFactory)
        {
            this.logger = logger;
        }

        public List<Journal> GetAllJournals(string userId)
        {
            using (DataContext)
            {
                return DataContext.Journals.Where(j => j.Id > 0 && j.UserId == userId).ToList();
            }
        }

        public Journal GetJournalById(int Id)
        {
            using (DataContext)
            {
                return DataContext.Journals.SingleOrDefault(j => j.Id == Id);
            }
        }

        public OperationStatus AddJournal(Journal newJournal)
        {
            return ExecuteOperations(
                context =>
                {
                    newJournal.ModifiedDate = DateTime.UtcNow;
                    newJournal.CreatedDate = DateTime.UtcNow;

                    var j = context.Data.Journals.Add(newJournal);
                    context.Data.Entry(j).State = EntityState.Added;
                },
                Save
                );
        }

        public OperationStatus DeleteJournal(Journal journal)
        {
            return ExecuteOperations(
                context =>
                {
                    var subscriptions = GetMany<Subscription>(s => s.JournalId == journal.Id);

                    foreach (var subscription in subscriptions)
                    {
                        context.Data.Entry(subscription).State = EntityState.Deleted;
                        context.Data.Subscriptions.Remove(subscription);
                    }

                    var journalToBeDeleted = Get<Journal>(j => j.Id == journal.Id);

                    context.Data.Entry(journalToBeDeleted).State = EntityState.Deleted;
                    context.Data.Journals.Remove(journalToBeDeleted);
                },
                Save
             );
        }

        public OperationStatus UpdateJournal(Journal journal)
        {
            Journal existingJournal = null;

            return ExecuteOperations(
                context =>
                {
                    existingJournal = context.Data.Journals.Find(journal.Id);

                    if (journal.Title != null)
                        existingJournal.Title = journal.Title;

                    if (journal.Description != null)
                        existingJournal.Description = journal.Description;

                    existingJournal.ModifiedDate = DateTime.UtcNow;

                    context.Data.Entry(existingJournal).State = EntityState.Modified;
                },
                Save);
        }

        public File GetFile(int id)
        {
            return Get<File>(f => f.Id == id);
        }

        public OperationStatus AddFile(File file)
        {
            return ExecuteOperations(
                context =>
                {
                    file.ModifiedDate = DateTime.UtcNow;
                },
                Add(file),
                Save
            );
        }

        public OperationStatus DeleteFile(int id)
        {
            return ExecuteOperations(
                Remove(Get<File>(f => f.Id == id)),
                Save
             );
        }

        public List<Issue> GetIssues(int journalId)
        {
            return GetMany<Issue>(i => i.JournalId == journalId).ToList();
        }

        public OperationStatus AddIssue(Issue issue)
        {
            throw new NotImplementedException();
        }

        public OperationStatus DeleteIssue(Issue issue)
        {
            throw new NotImplementedException();
        }

        public OperationStatus UpdateIssue(Issue issue)
        {
            throw new NotImplementedException();
        }
    }
}