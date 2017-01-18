using Journals.Model;
using Journals.Repository.DataContext;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
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

        public async Task<int> GetJournalCount()
        {
            return await Table<Journal>().CountAsync();
        }

        public List<Journal> GetAllJournals(string userId)
        {
            return GetMany<Journal>(j => j.Id > 0 && j.UserId == userId).ToList();
        }

        public Journal GetJournalById(int Id)
        {
            return Get<Journal>(j => j.Id == Id);
        }

        public OperationStatus AddJournal(Journal newJournal)
        {
            return ExecuteOperations(
                context =>
                {
                    newJournal.ModifiedDate = DateTime.UtcNow;
                    newJournal.CreatedDate = DateTime.UtcNow;

                    context.Data.Journals.Add(newJournal);
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
                        context.Data.Subscriptions.Remove(subscription);
                        context.Data.Entry(subscription).State = EntityState.Deleted;
                    }

                    var journalToBeDeleted = Get<Journal>(j => j.Id == journal.Id);

                    if (journalToBeDeleted != null)
                    {
                        context.Data.Journals.Remove(journalToBeDeleted);
                        context.Data.Entry(journalToBeDeleted).State = EntityState.Deleted;
                    }
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
                    existingJournal = context.Data.Journals.FirstOrDefault(j => j.Id == journal.Id);

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
            return ExecuteOperations(
                Add(issue),
                Save
                );
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