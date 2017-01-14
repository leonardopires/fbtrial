using AutoMapper;
using Journals.Model;
using Journals.Repository;
using Journals.Web.Controllers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using FluentAssertions;
using FluentAssertions.Mvc;
using Telerik.JustMock;
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

        private static PublisherController GetController(List<Journal> journals, Action<List<Journal>, IJournalRepository, MembershipUser> setupAction, string httpMethod = "GET")
        {
            Mapper.CreateMap<Journal, JournalViewModel>();

            //Arrange
            var membershipRepository = Mock.Create<IStaticMembershipService>();
            var userMock = Mock.Create<MembershipUser>();
            Mock.Arrange(() => userMock.ProviderUserKey).Returns(1);
            Mock.Arrange(() => membershipRepository.GetUser()).Returns(userMock);

            var journalRepository = Mock.Create<IJournalRepository>();

            setupAction?.Invoke(journals, journalRepository, userMock);

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
            Mock.Arrange(() => journalRepository.GetAllJournals((int) userMock.ProviderUserKey)).Returns(
                    journals);

            foreach(var journal in journals)
            {                
                Mock.Arrange(() => journalRepository.GetJournalById(journal.Id)).Returns(journal);
            }


            Mock.Arrange(() => journalRepository.GetJournalById(Arg.Matches<int>(i => journals.Count(item => item.Id == i) == 0))).Returns((Journal) null);

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

            result.Should().BeOfType<FileContentResult>();
            result.As<FileContentResult>().ContentType.Should().Be(expectedContentType);
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

            result.Should().BeAssignableTo<HttpStatusCodeResult>();
            result.As<HttpStatusCodeResult>().StatusCode.Should().Be(httpStatus);
        }

        [Theory]
        [MemberData(nameof(InvalidJournalViewModels))]
        public void Create_Post_With_Invalid_Args_Returns_View(JournalViewModel journal)
        {
            var controller = GetController(_defaultJournals, SetUpRepository);
            var result = controller.Create(journal);

            result.Should().BeViewResult();
        }

        public static IEnumerable<object[]> InvalidJournalViewModels
        {
            get
            {
                yield return new object[] { new JournalViewModel() };

            }
        }
    }
   
}