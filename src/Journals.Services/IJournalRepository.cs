using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Journals.Model;

namespace Journals.Services
{
    public interface IJournalRepository : IDisposable
    {
        List<Journal> GetAllJournals(string userId);

        OperationStatus AddJournal(Journal newJournal);

        Journal GetJournalById(int id);

        OperationStatus DeleteJournal(Journal journal);

        OperationStatus UpdateJournal(Journal journal);

        File GetFile(int id);

        OperationStatus AddFile(File file);

        OperationStatus DeleteFile(int id);

        List<Issue> GetIssues(int journalId);

        OperationStatus AddIssue(Issue issue);

        OperationStatus DeleteIssue(Issue issue);

        OperationStatus UpdateIssue(Issue issue);

        Task<int> GetJournalCount();

    }
}