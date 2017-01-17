using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using Journals.Model;
using LP.Test.Framework.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Serilog;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;

namespace Journals.Web.Tests.TestData
{
    public class JournalTestData : TestData<Journal>
    {


        public const string GUID_ONE = "00000000-0000-0000-0000-000000000001";
        public const string GUID_TWO = "00000000-0000-0000-0000-000000000002";

        public JournalTestData(ILogger logger)
        {
            Logger = logger;
        }

        public ILogger Logger { get; }


        public string DefaultUserId => GUID_ONE;

        public override List<Journal> GetDefaultData() => new List<Journal>
        {
            CreateJournal(),
            CreateJournal(2),
            CreateJournal(3, userId: GUID_TWO)
        };


        public IEnumerable<object[]> GetValidJournalViewModelsForCreate() =>
            Data(
                Item(
                    CreateJournalViewModel(
                        id: 3,
                        description: "TestDesc3",
                        title: "Tester3",
                        userId: GUID_ONE
                    )
                ),
                Item(
                    CreateJournalViewModel(
                        id: 4,
                        description: "TestDesc4",
                        title: "Tester4",
                        userId: GUID_TWO
                    )
                )
            );


        public IEnumerable<object[]> GetInvalidJournalViewModels() =>
            Data(

                Item(new JournalViewModel(), HttpStatusCode.OK),
                Item(
                    CreateJournalViewModel(
                        id: 30,
                        title: string.Empty
                    ),
                    HttpStatusCode.OK
                ),
                Item(
                    CreateJournalViewModel(
                        id: 4
                    ),
                    HttpStatusCode.OK
                ),
                Item(
                    CreateJournalViewModel(
                        id: 5
                    ),
                    HttpStatusCode.OK
                ),
                Item(
                    CreateJournalViewModel(
                        id: 6,
                        description: string.Empty
                    ),
                    HttpStatusCode.OK
                ),
                Item(
                    CreateJournalViewModel(
                        id: 7
                    ),
                    HttpStatusCode.OK
                ),
                Item(
                    CreateJournalViewModel(
                        id: 8
                    ),
                    HttpStatusCode.OK
                ),
                Item(
                    CreateJournalViewModel(
                        id: int.MaxValue
                    ),
                    HttpStatusCode.InternalServerError
                )
            );


        public IEnumerable<object[]> GetInvalidIdsAndExpectedStatusCodes() =>
            Data(
                Item(-1, HttpStatusCode.NotFound),
                Item(30, HttpStatusCode.NotFound),
                Item(4, HttpStatusCode.NotFound),
                Item(5, HttpStatusCode.NotFound),
                Item(6, HttpStatusCode.NotFound),
                Item(565, HttpStatusCode.NotFound),
                Item(int.MaxValue, HttpStatusCode.NotFound),
                Item(int.MinValue, HttpStatusCode.NotFound),
                Item(-55, HttpStatusCode.NotFound),
                Item(0, HttpStatusCode.NotFound),
                Item(32, HttpStatusCode.NotFound)
            );

        public IEnumerable<object[]> GetValidUpdatedJournals() =>
            Data(

                Item(
                    CreateJournalUpdateViewModel(
                        id: 1,
                        description: "DescriptionUpdated1",
                        title: "TesterUpdated1",
                        userId: GUID_TWO
                    ),
                    2
                ),
                Item(
                    CreateJournalUpdateViewModel(
                        id: 1,
                        description: "DescriptionUpdated1"
                    ),
                    2
                ),
                Item(
                    CreateJournalUpdateViewModel(
                        id: 1
                    ),
                    2
                ),
                Item(
                    CreateJournalUpdateViewModel(
                        id: 2
                    ),
                    2
                ),
                Item(
                    CreateJournalUpdateViewModel(
                        id: 1,
                        title: "TesterUpdated1"
                    ),
                    2
                ),
                Item(
                    CreateJournalUpdateViewModel(
                        id: 1,
                        userId: GUID_TWO
                    ),
                    2
                )
            );


        public IEnumerable<object[]> GetInvalidUpdatedJournals() =>
            Data(

                Item(
                    CreateJournalUpdateViewModel(
                    ),
                    HttpStatusCode.OK
                ),
                Item(
                    CreateJournalUpdateViewModel(
                        description: string.Empty
                    ),
                    HttpStatusCode.OK
                )
                ,
                Item(
                    CreateJournalUpdateViewModel(
                        title: string.Empty
                    ),
                    HttpStatusCode.OK
                )
                ,
                Item(
                    CreateJournalUpdateViewModel(
                    ),
                    HttpStatusCode.OK
                )
                ,
                Item(
                    CreateJournalUpdateViewModel(
                        int.MaxValue
                    ),
                    HttpStatusCode.InternalServerError
                )
            );


        public IEnumerable<object> GetFileIdsAndExpectedStatusCodes =>
            Data(
                Item(-1, HttpStatusCode.NotFound),
                Item(301, HttpStatusCode.NotFound),
                Item(0, HttpStatusCode.OK),
                Item(299, HttpStatusCode.OK),
                Item(300, HttpStatusCode.NotFound),
                Item(7, HttpStatusCode.OK),
                Item(154, HttpStatusCode.OK),
                Item(-7, HttpStatusCode.NotFound)
            );


        public JournalViewModel CreateJournalViewModel(
            int id = 1,
            string description = "TestDescription",
            string title = "Title",
            string userId = GUID_ONE,
            bool forceNullContent = false)
        {

            var journalViewModel = new JournalViewModel
            {
                Id = id,
                Description = description,
                Title = title,
                UserId = userId
            };
            return journalViewModel;
        }

        public JournalUpdateViewModel CreateJournalUpdateViewModel(
            int id = 1,
            string description = "TestDescription",
            string title = "Title",
            string userId = GUID_ONE,
            bool forceNullContent = false)
        {
            var journalViewModel = new JournalUpdateViewModel
            {
                Id = id,
                Description = description,
                Title = title,
                UserId = userId
            };
            return journalViewModel;
        }

        public IFormFile CreateFormFile(byte[] content, string fileName, string contentType)
        {
            var file = Mock.Create<IFormFile>();

            file.Arrange(f => f.OpenReadStream()).Returns(content != null ? new MemoryStream(content) : Stream.Null);
            file.Arrange(f => f.FileName).Returns(fileName);
            file.Arrange(f => f.ContentType).Returns(contentType);
            file.Arrange(f => f.Length).Returns(content?.Length ?? -1);
            return file;
        }

        public Journal CreateJournal(
            int id = 1,
            byte[] content = null,
            string description = "TestDescription",
            string fileName = "FileName.pdf",
            string contentType = "application/pdf",
            string title = "Title",
            string userId = GUID_ONE,
            DateTime? modifiedDate = null,
            bool forceNullContent = false
            )
        {
            var journalViewModel = new Journal
            {
                Id = id,
                Description = description,
                Title = title,
                UserId = userId,
                ModifiedDate = modifiedDate ?? DateTime.UtcNow
            };
            return journalViewModel;
        }


        /// <summary>
        ///     Gets the files to upload.
        /// </summary>
        /// <returns>
        ///     <see cref="IEnumerable{object[]}" />
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
