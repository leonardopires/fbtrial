using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Web;
using Journals.Model;
using Journals.Repository;
using Journals.Web.Tests.Framework;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using LP.Test.Framework.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Serilog;

namespace Journals.Web.Tests.TestData
{
    public class JournalTestData : TestData<Journal>
    {

        public ILogger Logger { get; }

        public JournalTestData(ILogger logger)
        {
            Logger = logger;
        }

        public override List<Journal> GetDefaultData() => new List<Journal>
        {
            CreateJournal(),
            CreateJournal(2),
            CreateJournal(3, userId: 2)
        };


        public IEnumerable<object[]> GetValidJournalViewModelsForCreate() => new List<object[]>
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
                    userId: 2
                )
            }
        };


        public IEnumerable<object[]> GetInvalidJournalViewModels() => new List<object[]>
        {
            new object[] {new JournalViewModel(), HttpStatusCode.OK},
            new object[]
            {
                CreateJournalViewModel(
                    30,
                    title: string.Empty
                ),
                HttpStatusCode.OK
            },
            new object[]
            {
                CreateJournalViewModel(
                    4,
                    fileName: string.Empty
                ),
                HttpStatusCode.OK
            },
            new object[]
            {
                CreateJournalViewModel(
                    5,
                    contentType: string.Empty
                ),
                HttpStatusCode.OK
            },
            new object[]
            {
                CreateJournalViewModel(
                    6,
                    description: string.Empty
                ),
                HttpStatusCode.OK
            },
            new object[]
            {
                CreateJournalViewModel(
                    7,
                    fileName: "TestFilename7.jpg"
                ),
                HttpStatusCode.OK
            },
            new object[]
            {
                CreateJournalViewModel(
                    8,
                    new byte[1024 * 1024 * 4]
                ),
                HttpStatusCode.OK
            },
            new object[]
            {
                CreateJournalViewModel(
                    int.MaxValue
                ),
                HttpStatusCode.InternalServerError
            }
        };


        public IEnumerable<object[]> GetInvalidIdsAndExpectedStatusCodes() => new List<object[]>
        {
            new object[] {-1, HttpStatusCode.NotFound},
            new object[] {30, HttpStatusCode.NotFound},
            new object[] {4, HttpStatusCode.NotFound},
            new object[] {5, HttpStatusCode.NotFound},
            new object[] {6, HttpStatusCode.NotFound},
            new object[] {565, HttpStatusCode.NotFound},
            new object[] {int.MaxValue, HttpStatusCode.NotFound},
            new object[] {int.MinValue, HttpStatusCode.NotFound},
            new object[] {-55, HttpStatusCode.NotFound},
            new object[] {0, HttpStatusCode.NotFound},
            new object[] {32, HttpStatusCode.NotFound}
        };

        public IEnumerable<object[]> GetValidUpdatedJournals() => new List<object[]>
        {
            new object[]
            {
                CreateJournalUpdateViewModel(
                    content: new byte[1024 * 1024 * 3],
                    id: 1,
                    description: "DescriptionUpdated1",
                    fileName: "TestFilenameUpdated1.pdf",
                    contentType: "application/pdf",
                    title: "TesterUpdated1",
                    userId: 2
                ), 2
            },
            new object[]
            {
                CreateJournalUpdateViewModel(
                    1,
                    description: "DescriptionUpdated1"
                ), 2
            },
            new object[]
            {
                CreateJournalUpdateViewModel(
                    1,
                    fileName: "TestFilenameUpdated1.pdf"
                ), 2
            },
            new object[]
            {
                CreateJournalUpdateViewModel(
                    2,
                    contentType: "application/octet-stream"
                ), 2 
            },
            new object[]
            {
                CreateJournalUpdateViewModel(
                    1,
                    title: "TesterUpdated1"
                ), 2
            },
            new object[]
            {
                CreateJournalUpdateViewModel(
                    1,
                    userId: 2
                ), 2
            }
        };


        public IEnumerable<object[]> GetInvalidUpdatedJournals() => new List<object[]>
        {
            new object[]
            {
                CreateJournalUpdateViewModel(
                    fileName: string.Empty
                ),
                HttpStatusCode.OK
            },
            new object[]
            {
                CreateJournalUpdateViewModel(
                    description: string.Empty
                ),
                HttpStatusCode.OK
            },
            new object[]
            {
                CreateJournalUpdateViewModel(
                    title: string.Empty
                ),
                HttpStatusCode.OK
            },
            new object[]
            {
                CreateJournalUpdateViewModel(
                    content: new byte[1024 * 1024 * 4]
                ),
                HttpStatusCode.OK
            },
            new object[]
            {
                CreateJournalUpdateViewModel(
                    int.MaxValue
                ),
                HttpStatusCode.InternalServerError
            }
        };

        public JournalViewModel CreateJournalViewModel(
            int id = 1,
            byte[] content = null,
            string description = "TestDescription",
            string fileName = "FileName.pdf",
            string contentType = "application/pdf",
            string title = "Title",
            int userId = 1,
            bool forceNullContent = false)
        {
            //var file = CreateHttpPostedFile(content, fileName, contentType);

            var journalViewModel = new JournalViewModel
            {
                Id = id,
                Description = description,
                FileName = fileName,
                ContentType = contentType,
                Content = content ?? (forceNullContent ? null : new byte[1]),
                Title = title,
                UserId = userId,
                //File = file
            };
            return journalViewModel;
        }

        public JournalUpdateViewModel CreateJournalUpdateViewModel(
            int id = 1,
            byte[] content = null,
            string description = "TestDescription",
            string fileName = "FileName.pdf",
            string contentType = "application/pdf",
            string title = "Title",
            int userId = 1,
            bool forceNullContent = false)
        {
            //var file = CreateHttpPostedFile(content, fileName, contentType);

            var journalViewModel = new JournalUpdateViewModel
            {
                Id = id,
                Description = description,
                FileName = fileName,
                ContentType = contentType,
                Content = content ?? (forceNullContent ? null : new byte[1]),
                Title = title,
                UserId = userId,
                //File = file
            };
            return journalViewModel;
        }

        //public HttpPostedFileBase CreateHttpPostedFile(byte[] content, string fileName, string contentType)
        //{
        //    var file = Mock.Create<HttpPostedFileBase>();

        //    file.Arrange(f => f.InputStream).Returns(content != null ? new MemoryStream(content) : Stream.Null);
        //    file.Arrange(f => f.FileName).Returns(fileName);
        //    file.Arrange(f => f.ContentType).Returns(contentType);
        //    file.Arrange(f => f.ContentLength).Returns(content?.Length ?? -1);
        //    return file;
        //}

        public Journal CreateJournal(
            int id = 1,
            byte[] content = null,
            string description = "TestDescription",
            string fileName = "FileName.pdf",
            string contentType = "application/pdf",
            string title = "Title",
            int userId = 1,
            DateTime? modifiedDate = null,
            bool forceNullContent = false)
        {
            var journalViewModel = new Journal
            {
                Id = id,
                Description = description,
                FileName = fileName,
                ContentType = contentType,
                Content = content ?? (forceNullContent ? null : new byte[1]),
                Title = title,
                UserId = userId,
                ModifiedDate = modifiedDate ?? DateTime.UtcNow
            };
            return journalViewModel;
        }


        /// <summary>
        /// Gets the files to upload.
        /// </summary>
        /// <returns>
        ///   <see cref="IEnumerable{object[]}" />
        /// </returns>
        public IEnumerable<object[]> GetFilesToUpload()
        {
            return Data(            
                Item(CreateFormFileFromLocalFile("1/09628d25-ea42-490e-965d-cd4ffb6d4e9d.pdf")),
                Item(CreateFormFileFromLocalFile("1/8305d848-88d2-4cbd-a33b-5c3dcc548056.pdf")),
                Item(CreateFormFileFromLocalFile("2/75f29692-237b-4116-95ed-645de5c57b4d.pdf"))
            );
        }

        private IFormFile CreateFormFileFromLocalFile(string fileName)
        {        
            var file = new FileInfo(Path.Combine(Environment.CurrentDirectory, "Testfiles", fileName));

            Logger.Debug("{@file}", file);

            Stream baseStream = new MemoryStream();

            using (var readStream = file.Open(FileMode.Open, FileAccess.Read, FileShare.Delete))
            {
                readStream.CopyTo(baseStream);
            }

            IFormFile formFile = new FormFile(baseStream, 0, baseStream.Length, file.Name, file.Name);
                return formFile;
        }

    }
}
