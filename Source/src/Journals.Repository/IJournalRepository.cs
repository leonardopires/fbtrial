using System;
using Journals.Model;
using System.Collections.Generic;

namespace Journals.Repository
{
    public interface IJournalRepository : IDisposable
    {
        List<Journal> GetAllJournals(string userId);

        OperationStatus AddJournal(Journal newJournal);

        Journal GetJournalById(int Id);

        OperationStatus DeleteJournal(Journal journal);

        OperationStatus UpdateJournal(Journal journal);
    }
}