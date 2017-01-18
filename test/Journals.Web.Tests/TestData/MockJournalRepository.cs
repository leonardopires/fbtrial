using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Journals.Model;
using Journals.Repository;
using Journals.Services;
using LP.Test.Framework.Core;
using Serilog;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using File = Journals.Model.File;

namespace Journals.Web.Tests.TestData
{
    public class MockJournalRepository : MockRepositoryWrapper<Journal, IJournalRepository>, IJournalRepository
    {

        public MockJournalRepository(ITestData<Journal> testData) : base(testData)
        {
        }

        public override void ArrangeMock()
        {
            mock.Arrange(r => r.GetAllJournals(Arg.IsAny<string>())).Returns((string id) => models.Where(i => i.UserId == id).ToList());

            mock.Arrange(r => r.GetJournalById(Arg.IsAny<int>())).Returns((int id) => models.FirstOrDefault(j => j.Id == id));



            mock.Arrange(i => i.AddJournal(Arg.IsAny<Journal>()))
                .Returns(
                    (Journal a) =>
                    {
                        models.Add(a);
                        return new OperationStatus {Status = a.Id != int.MaxValue};
                    });


            mock.Arrange(i => i.DeleteJournal(Arg.IsAny<Journal>()))
                .Returns(
                    (Journal a) =>
                    {
                        var modelToRemove = models.FirstOrDefault(i => i.Id == a.Id);
                        var status = new OperationStatus
                        {
                            Status = (modelToRemove != null) && models.Remove(modelToRemove)
                        };
                        return status;
                    });


            mock.Arrange(i => i.UpdateJournal(Arg.IsAny<Journal>()))
                .Returns(
                    (Journal a) =>
                    {
                        var index = models.FindIndex(i => i.Id == a.Id);

                        if (index >= 0)
                        {
                            var model = models[index];

                            model.Description = a.Description;
                            model.ModifiedDate = a.ModifiedDate;
                            model.Title = a.Title;
                            model.UserId = a.UserId;
                            model.CreatedDate = a.CreatedDate;

                            models[index] = model;
                        }
                        return new OperationStatus {Status = index >= 0};
                    });

            mock.Arrange(m => m.GetFile(Arg.AnyInt)).Returns(
                (int id) => 
                    (id >= 0 && id < 300) 
                    ? new File {Id = id, Content = new byte[id], Length = id, ContentType = "application/pdf", FileName = $"File_{id}.pdf", ModifiedDate = DateTime.UtcNow} 
                    : null
                 );

            mock.Arrange(m => m.AddFile(Arg.IsAny<File>())).Returns(
                (File file) =>
                {
                    OperationStatus status;

                    try
                    {
                        using (var stream = new MemoryStream(file.Content))
                        {
                            var directory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, "Uploads"));
                            directory.Create();

                            var localFile = new FileInfo(Path.Combine(directory.ToString(), $"{DateTime.UtcNow:yyyyMMdd-hhmmss}_{file.FileName}"));


                            using (var fileStream = localFile.Open(FileMode.OpenOrCreate, FileAccess.Write, FileShare.Delete))
                            {
                                stream.CopyTo(fileStream);

                                Log.Logger.Debug($"Saved file {localFile.FullName}");

                                status = new OperationStatus() { Status = true };
                            } 

                        }
                    }
                    catch (IOException ex)
                    {
                        status = OperationStatus.CreateFromException("An error occurred while saving the uploaded file.", ex);
                    }
                    return status;
                });

            mock.Arrange(m => m.GetJournalCount()).Returns(() => Task.FromResult(10));

        }

        public List<Journal> GetAllJournals(string userId)
        {
            return mock.GetAllJournals(userId);
        }

        public OperationStatus AddJournal(Journal newJournal)
        {
            return mock.AddJournal(newJournal);
        }

        public Journal GetJournalById(int Id)
        {
            return mock.GetJournalById(Id);
        }

        public OperationStatus DeleteJournal(Journal journal)
        {
            return mock.DeleteJournal(journal);
        }

        public OperationStatus UpdateJournal(Journal journal)
        {
            return mock.UpdateJournal(journal);
        }

        public File GetFile(int id)
        {
            return mock.GetFile(id);
        }

        public OperationStatus AddFile(File file)
        {
            return mock.AddFile(file);
        }

        public OperationStatus DeleteFile(int id)
        {
            return mock.DeleteFile(id);
        }

        public List<Issue> GetIssues(int journalId)
        {
            return mock.GetIssues(journalId);
        }

        public OperationStatus AddIssue(Issue issue)
        {
            return mock.AddIssue(issue);
        }

        public OperationStatus DeleteIssue(Issue issue)
        {
            return mock.DeleteIssue(issue);
        }

        public OperationStatus UpdateIssue(Issue issue)
        {
            return mock.UpdateIssue(issue);
        }

        public async Task<int> GetJournalCount()
        {
            return await mock.GetJournalCount();
        }

    }
}