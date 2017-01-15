using System.Collections.Generic;
using System.Linq;
using Journals.Model;
using Journals.Repository;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Journals.Web.Tests.TestData
{
    public class MockJournalRepository : IJournalRepository
    {
        private readonly IJournalRepository mock;
        private readonly List<Journal> models;


        public MockJournalRepository(ITestData<Journal> testData)
        {
            models = new List<Journal>(testData.GetDefaultData());

            mock = Mock.Create<IJournalRepository>();

            ArrangeMock();
        }

        private void ArrangeMock()
        {
            mock.Arrange(r => r.GetAllJournals(Arg.IsAny<int>())).Returns((int id) => models.Where(i => i.UserId == id).ToList());

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

                            model.Content = a.Content;
                            model.ContentType = a.ContentType;
                            model.FileName = a.FileName;
                            model.Description = a.Description;
                            model.ModifiedDate = a.ModifiedDate;
                            model.Title = a.Title;
                            model.UserId = a.UserId;

                            models[index] = model;
                        }
                        return new OperationStatus {Status = index >= 0};
                    });
        }

        public void Dispose()
        {
            mock.Dispose();
        }

        public List<Journal> GetAllJournals(int userId)
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

    }
}