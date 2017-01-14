using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Journals.Model;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Journals.Web.Tests.Data
{
    public class JournalTestData
    {
        public List<Journal> DefaultData => new List<Journal>()
        {
            new Journal
            {
                Id = 1,
                Description = "TestDesc",
                FileName = "TestFilename.pdf",
                ContentType = "application/pdf",
                Content = new byte[1],
                Title = "Tester",
                UserId = 1,
                ModifiedDate = DateTime.Now
            },
            new Journal
            {
                Id = 2,
                Description = "TestDesc2",
                FileName = "TestFilename2.pdf",
                ContentType = "application/pdf",
                Content = new byte[1],
                Title = "Tester2",
                UserId = 1,
                ModifiedDate = DateTime.Now
            }
        };


        public IEnumerable<object[]> ValidJournalViewModelsForCreate => new List<object[]>
        {
            new object[]
            {
                CreateJournalViewModel(
                    content: new byte[0],
                    id: 3,
                    description: "TestDesc3",
                    fileName: "TestFilename3.pdf",
                    contentType: "application/pdf",
                    title: "Tester3",
                    userId: 1
                )
            },

            new object[]
            {
                CreateJournalViewModel(
                    content: new byte[0],
                    id: 4,
                    description: "TestDesc4",
                    fileName: "TestFilename4.pdf",
                    contentType: "application/pdf",
                    title: "Tester4",
                    userId: 1
                )
            }
        };


        public IEnumerable<object[]> InvalidJournalViewModels => new List<object[]>
        {
            new object[] {new JournalViewModel(), HttpStatusCode.OK},

            new object[]
            {
                CreateJournalViewModel(
                    content: new byte[1],
                    id: 3,
                    description: "TestDesc3",
                    fileName: "TestFilename3.pdf",
                    contentType: "application/pdf",
                    title: "",
                    userId: 1

                ),
                HttpStatusCode.OK
            },
            new object[]
            {
                CreateJournalViewModel(
                    content: new byte[1],
                    id: 4,
                    description: "TestDesc4",
                    fileName: "",
                    contentType: "application/pdf",
                    title: "Tester4",
                    userId: 1

                ),
                HttpStatusCode.OK
            },
            new object[]
            {
                CreateJournalViewModel(
                    content: new byte[1],
                    id: 5,
                    description: "TestDesc5",
                    fileName: "TestFilename5.pdf",
                    contentType: "",
                    title: "Tester5",
                    userId: 1

                ),
                HttpStatusCode.OK
            },
            new object[]
            {
                CreateJournalViewModel(
                    content: new byte[1],
                    id: 6,
                    description: "",
                    fileName: "TestFilename6.pdf",
                    contentType: "application/pdf",
                    title: "Tester6",
                    userId: 1

                ),
                HttpStatusCode.OK
            },
            new object[]
            {
                CreateJournalViewModel(
                    content: new byte[1],
                    id: 7,
                    description: "Description7",
                    fileName: "TestFilename7.jpg",
                    contentType: "application/pdf",
                    title: "Tester7",
                    userId: 1

                ),
                HttpStatusCode.OK
            },
            new object[]
            {
                CreateJournalViewModel(
                    content: new byte[1],
                    id: int.MaxValue,
                    description: "DescriptionN",
                    fileName: "TestFilenameN.pdf",
                    contentType: "application/pdf",
                    title: "TesterN",
                    userId: 1

                ),
                HttpStatusCode.InternalServerError
            }
        };

        private static JournalViewModel CreateJournalViewModel(
            byte[] content,
            int id,
            string description,
            string fileName,
            string contentType,
            string title,
            int userId)
        {
            var file = Mock.Create<HttpPostedFileBase>();

            file.Arrange((f) => f.InputStream).Returns(new MemoryStream(content));
            file.Arrange((f) => f.FileName).Returns(fileName);
            file.Arrange((f) => f.ContentType).Returns(contentType);
            file.Arrange((f) => f.ContentLength).Returns(content.Length);

            var journalViewModel = new JournalViewModel()
            {
                Id = id,
                Description = description,
                FileName = fileName,
                ContentType = contentType,
                Content = content,
                Title = title,
                UserId = userId,
                File = file
            };
            return journalViewModel;
        }

    }
}
