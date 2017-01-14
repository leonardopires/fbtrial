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
using Journals.Web.Tests.Data;
using Microsoft.Data.OData.Query.SemanticAst;
using Newtonsoft.Json;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace Journals.Web.Tests.Controllers
{
    public class PublisherControllerTest : IDisposable
    {

        private static readonly JournalTestData data;

        public PublisherControllerTest()
        {
            Mapper.CreateMap<Journal, JournalViewModel>();
            Mapper.CreateMap<JournalViewModel, Journal>();

            Mapper.CreateMap<Journal, JournalUpdateViewModel>();
            Mapper.CreateMap<JournalUpdateViewModel, Journal>();

            Mapper.CreateMap<Journal, SubscriptionViewModel>();
            Mapper.CreateMap<SubscriptionViewModel, Journal>();
        }

        static PublisherControllerTest()
        {
            data = new JournalTestData();
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

            journalRepository.Arrange((r) => r.GetAllJournals((int)userMock.ProviderUserKey)).Returns(journals);

            foreach (var journal in journals)
            {
                journalRepository.Arrange((r) => r.GetJournalById(journal.Id)).Returns(journal);
            }


            journalRepository.Arrange((r) => r.GetJournalById(Arg.Matches<int>(i => journals.Count(item => item.Id == i) == 0))).Returns((Journal)null);

            journalRepository.Arrange(i => i.AddJournal(Arg.IsAny<Journal>()))
                                                .Returns(
                                                    (Journal a) =>
                                                    {
                                                        journals.Add(a);
                                                        return new OperationStatus() { Status = a.Id != int.MaxValue };
                                                    });

        }


        [Theory]
        [InlineData(2)]
        public void Index_Returns_All_Journals(int count)
        {
            var controller = GetController(DefaultData, SetUpRepository);

            ViewResult actionResult = (ViewResult)controller.Index();
            var model = actionResult.Model as IEnumerable<JournalViewModel>;

            //Assert
            Assert.NotNull(model);

            Assert.Equal(count, model.Count());
        }


        [Fact]
        public void Create_Returns_View_On_Get()
        {
            var controller = GetController(DefaultData, SetUpRepository);

            var result = controller.Create();

            result.Should().BeViewResult();
        }

        [Theory]
        [InlineData(1, "application/pdf")]
        [InlineData(2, "application/pdf")]
        public void GetFile_Returns_File(int fileId, string expectedContentType)
        {
            var controller = GetController(DefaultData, SetUpRepository);
            var result = controller.GetFile(fileId);

            result.Should().BeOfType<FileContentResult>().Which.ContentType.Should().Be(expectedContentType);
        }

        [Theory]
        [InlineData(-1, HttpStatusCode.NotFound)]
        [InlineData(3, HttpStatusCode.NotFound)]
        [InlineData(4, HttpStatusCode.NotFound)]
        [InlineData(5, HttpStatusCode.NotFound)]
        [InlineData(6, HttpStatusCode.NotFound)]
        [InlineData(565, HttpStatusCode.NotFound)]
        [InlineData(int.MaxValue, HttpStatusCode.NotFound)]
        [InlineData(int.MinValue, HttpStatusCode.NotFound)]
        [InlineData(-55, HttpStatusCode.NotFound)]
        [InlineData(0, HttpStatusCode.NotFound)]
        [InlineData(32, HttpStatusCode.NotFound)]
        public void GetFile_Returns_StatusCode_OnErrors(int fileId, HttpStatusCode httpStatus)
        {
            var controller = GetController(DefaultData, SetUpRepository);
            var result = controller.GetFile(fileId);

            result.Should().BeAssignableTo<HttpStatusCodeResult>().Which.StatusCode.Should().Be((int)httpStatus);
        }

        [Theory]
        [MemberData(nameof(InvalidJournalViewModels))]
        public void Create_Post_With_Invalid_Args_Returns_StatusCode(JournalViewModel journal, HttpStatusCode statusCode)
        {
            var controller = GetController(DefaultData, SetUpRepository);

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
            var controller = GetController(DefaultData, SetUpRepository);

            controller.ValidateViewModel(journal);

            var result = controller.Create(journal);

            controller.ModelState.IsValid.Should().BeTrue("ModelState should be valid");

            result.Should().BeRedirectToRouteResult().WithAction("Index");
        }

        public static IEnumerable<object[]> InvalidJournalViewModels => data.InvalidJournalViewModels;

        public static IEnumerable<object[]> ValidJournalViewModelsForCreate => data.ValidJournalViewModelsForCreate;

        public static List<Journal> DefaultData => data.DefaultData;

        public void Dispose()
        {
            Mapper.Reset();
        }

    }
}