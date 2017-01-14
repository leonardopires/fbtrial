using AutoMapper;
using Journals.Model;
using Journals.Repository;
using Journals.Web.Controllers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using FluentAssertions;
using FluentAssertions.Mvc;
using Microsoft.Data.OData.Query.SemanticAst;
using Newtonsoft.Json;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace Journals.Web.Tests.Controllers
{
    public class PublisherControllerTest
    {

        private static List<Journal> _defaultJournals = new List<Journal>()
            {
                new Journal
                {
                    Id = 1,
                    Description = "TestDesc",
                    FileName = "TestFilename.pdf",
                    ContentType = "application/pdf",
                    Content = new byte[0],
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
                    Content = new byte[0],
                    Title = "Tester2",
                    UserId = 1,
                    ModifiedDate = DateTime.Now
                }
            };


        public PublisherControllerTest()
        {
            Mapper.CreateMap<Journal, JournalViewModel>();
            Mapper.CreateMap<JournalViewModel, Journal>();

            Mapper.CreateMap<Journal, JournalUpdateViewModel>();
            Mapper.CreateMap<JournalUpdateViewModel, Journal>();

            Mapper.CreateMap<Journal, SubscriptionViewModel>();
            Mapper.CreateMap<SubscriptionViewModel, Journal>();
        }

        private static PublisherController GetController(List<Journal> journals, Action<List<Journal>, IJournalRepository, MembershipUser> setupAction, string httpMethod = "GET")
        {
            Mapper.CreateMap<Journal, JournalViewModel>();

            //Arrange
            var membershipRepository = Mock.Create<IStaticMembershipService>();
            var userMock = Mock.Create<MembershipUser>();
            Mock.Arrange(() => userMock.ProviderUserKey).Returns(1);
            Mock.Arrange(() => membershipRepository.GetUser()).Returns(userMock);

            var journalRepository = Mock.Create<IJournalRepository>();

            var journalsCopy = new List<Journal>(journals);

            setupAction?.Invoke(journalsCopy, journalRepository, userMock);

            //Act
            PublisherController controller = new PublisherController(journalRepository, membershipRepository);

            var mockHttpContext = Mock.Create<HttpContextBase>();
            var mockRequest = Mock.Create<HttpRequestBase>();

            Mock.Arrange(() => mockHttpContext.Request).Returns(mockRequest);
            Mock.Arrange(() => mockRequest.HttpMethod).Returns(httpMethod);

            controller.ControllerContext = new ControllerContext(mockHttpContext, new RouteData(), controller);

            return controller;
        }

        private static void SetUpRepository(List<Journal> journals, IJournalRepository journalRepository, MembershipUser userMock)
        {
            Mock.Arrange(() => journalRepository.GetAllJournals((int)userMock.ProviderUserKey)).Returns(
                    journals);

            foreach (var journal in journals)
            {
                Mock.Arrange(() => journalRepository.GetJournalById(journal.Id)).Returns(journal);
            }


            Mock.Arrange(() => journalRepository.GetJournalById(Arg.Matches<int>(i => journals.Count(item => item.Id == i) == 0))).Returns((Journal)null);            

        }


        [Theory]
        [InlineData(2)]
        public void Index_Returns_All_Journals(int count)
        {
            var controller = GetController(_defaultJournals, SetUpRepository);

            ViewResult actionResult = (ViewResult)controller.Index();
            var model = actionResult.Model as IEnumerable<JournalViewModel>;

            //Assert
            Assert.NotNull(model);

            Assert.Equal(count, model.Count());
        }


        [Fact]
        public void Create_Returns_View_On_Get()
        {
            var controller = GetController(_defaultJournals, SetUpRepository);

            var result = controller.Create();

            result.Should().BeViewResult();
        }

        [Theory]
        [InlineData(1, "application/pdf")]
        [InlineData(2, "application/pdf")]
        public void GetFile_Returns_File(int fileId, string expectedContentType)
        {
            var controller = GetController(_defaultJournals, SetUpRepository);
            var result = controller.GetFile(fileId);

            result.Should().BeOfType<FileContentResult>().Which.ContentType.Should().Be(expectedContentType);
        }

        [Theory]
        [InlineData(-1, 404)]
        [InlineData(3, 404)]
        [InlineData(4, 404)]
        [InlineData(5, 404)]
        [InlineData(6, 404)]
        [InlineData(565, 404)]
        [InlineData(int.MaxValue, 404)]
        [InlineData(int.MinValue, 404)]
        [InlineData(-55, 404)]
        [InlineData(0, 404)]
        [InlineData(32, 404)]
        public void GetFile_Returns_StatusCode_OnErrors(int fileId, int httpStatus)
        {
            var controller = GetController(_defaultJournals, SetUpRepository);
            var result = controller.GetFile(fileId);

            result.Should().BeAssignableTo<HttpStatusCodeResult>().Which.StatusCode.Should().Be(httpStatus);
        }

        [Theory]
        [MemberData(nameof(InvalidJournalViewModels))]
        public void Create_Post_With_Invalid_Args_Returns_StatusCode(JournalViewModel journal, HttpStatusCode statusCode)
        {
            var controller = GetController(_defaultJournals, SetUpRepository);

            controller.ValidateViewModel(journal);
            var result = controller.Create(journal);


            if (statusCode != HttpStatusCode.OK)
            {
                controller.ModelState.IsValid.Should().BeTrue("ModelState should be valid");
                result.Should().BeAssignableTo<HttpStatusCodeResult>().Which.StatusCode.Should().Be((int) statusCode, "the error should have happened in the repository");
            }
            else
            {
                controller.ModelState.IsValid.Should().BeFalse("ModelState should be invalid");
                result.Should().NotBeOfType<HttpStatusCodeResult>("validation errors should be identified by ModelState");
                result.Should().BeViewResult("validation errors should appear to the user");
            }
        }

        [Theory]
        [MemberData(nameof(ValidJournalViewModelsForCreate))]
        public void Create_Post_With_Valid_Args_Redirects_To_Index(JournalViewModel journal)
        {
            List<Journal> journals = null;
            int count = int.MinValue;

            var controller = GetController(_defaultJournals,
                                           (j, r, u) =>
                                           {
                                               SetUpRepository(j, r, u);
                                               r.Arrange(i => i.AddJournal(Arg.IsAny<Journal>()))
                                                .Returns(
                                                    (Journal a) =>
                                                    {
                                                        j.Add(a);                                                        
                                                        return new OperationStatus() {Status = true};
                                                    }).MustBeCalled();

                                               journals = j;
                                               count = journals.Count;
                                           });

            controller.ValidateViewModel(journal);

            var result = controller.Create(journal);

            result.Should().BeRedirectToRouteResult().WithAction("Index");

            journals.Should().HaveCount(count + 1);

        }

        public static IEnumerable<object[]> InvalidJournalViewModels
        {
            get
            {
                yield return new object[] { new JournalViewModel(), HttpStatusCode.OK };

                yield return new object[] {
                    CreateJournalViewModel(
                        content: new byte[1],
                        id: 3,
                        description: "TestDesc3",
                        fileName: "TestFilename3.pdf",
                        contentType: "application/pdf",
                        title: "",
                        userId: 1
                       
                        ), HttpStatusCode.OK
                };

                yield return new object[] {
                    CreateJournalViewModel(
                        content: new byte[1],
                        id: 4,
                        description: "TestDesc4",
                        fileName: "",
                        contentType: "application/pdf",
                        title: "Tester4",
                        userId: 1

                        ), HttpStatusCode.OK
                };

                yield return new object[] {
                    CreateJournalViewModel(
                        content: new byte[1],
                        id: 5,
                        description: "TestDesc5",
                        fileName: "TestFilename5.pdf",
                        contentType: "",
                        title: "Tester5",
                        userId: 1

                        ), HttpStatusCode.OK
                };


                yield return new object[] {
                    CreateJournalViewModel(
                        content: new byte[1],
                        id: 6,
                        description: "",
                        fileName: "TestFilename6.pdf",
                        contentType: "application/pdf",
                        title: "Tester6",
                        userId: 1

                        ), HttpStatusCode.OK
                };


                yield return new object[] {
                    CreateJournalViewModel(
                        content: new byte[1],
                        id: 7,
                        description: "Description7",
                        fileName: "TestFilename7.jpg",
                        contentType: "application/pdf",
                        title: "Testert",
                        userId: 1

                        ), HttpStatusCode.OK
                };

            }
        }

        public static IEnumerable<object[]> ValidJournalViewModelsForCreate
        {
            get
            {

                yield return new object[] {
                    CreateJournalViewModel(
                        content: new byte[0],
                        id: 3,
                        description: "TestDesc3",
                        fileName: "TestFilename3.pdf",
                        contentType: "application/pdf",
                        title: "Tester3",
                        userId: 1
                        )
                };

                yield return new object[] {
                    CreateJournalViewModel(
                        content: new byte[0],
                        id: 4,
                        description: "TestDesc4",
                        fileName: "TestFilename4.pdf",
                        contentType: "application/pdf",
                        title: "Tester4",
                        userId: 1
                        )
                };
            }
        }

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