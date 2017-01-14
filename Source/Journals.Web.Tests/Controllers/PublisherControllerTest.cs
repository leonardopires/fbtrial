using AutoMapper;
using Journals.Model;
using Journals.Repository;
using Journals.Web.Controllers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http.Controllers;
using System.Web.Mvc;
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
    public class PublisherControllerTest : ControllerTest<PublisherController, Journal, IJournalRepository, JournalTestData>
    {

        public PublisherControllerTest()
        {
            Mapper.CreateMap<Journal, JournalViewModel>();
            Mapper.CreateMap<JournalViewModel, Journal>();

            Mapper.CreateMap<Journal, JournalUpdateViewModel>();
            Mapper.CreateMap<JournalUpdateViewModel, Journal>();

            Mapper.CreateMap<Journal, SubscriptionViewModel>();
            Mapper.CreateMap<SubscriptionViewModel, Journal>();
        }

        protected override PublisherController CreateControllerInstance(
            IJournalRepository repository,
            IStaticMembershipService membershipService)
        {
            return new PublisherController(repository, membershipService);
        }


        public static IEnumerable<object[]> InvalidJournalViewModels => Data.InvalidJournalViewModels;

        public static IEnumerable<object[]> ValidJournalViewModelsForCreate => Data.ValidJournalViewModelsForCreate;

        public static List<Journal> DefaultData => Data.DefaultData;


        protected override void SetUpRepository(List<Journal> models, IJournalRepository modelRepository, MembershipUser userMock)
        {

            modelRepository.Arrange((r) => r.GetAllJournals((int)userMock.ProviderUserKey)).Returns(models);

            foreach (var journal in models)
            {
                modelRepository.Arrange((r) => r.GetJournalById(journal.Id)).Returns(journal);
            }


            modelRepository.Arrange((r) => r.GetJournalById(Arg.Matches<int>(i => models.Count(item => item.Id == i) == 0))).Returns((Journal)null);

            modelRepository.Arrange(i => i.AddJournal(Arg.IsAny<Journal>()))
                                                .Returns(
                                                    (Journal a) =>
                                                    {
                                                        models.Add(a);
                                                        return new OperationStatus() { Status = a.Id != int.MaxValue };
                                                    });


            modelRepository.Arrange(i => i.DeleteJournal(Arg.IsAny<Journal>()))
                                                .Returns(
                                                    (Journal a) =>
                                                    {
                                                        var modelToRemove = models.FirstOrDefault(i => i.Id == a.Id);
                                                        var status = new OperationStatus() {Status = modelToRemove != null && models.Remove(modelToRemove) };
                                                        return status;
                                                    });

        }


        [Theory]
        [InlineData(2)]
        public void Index_Returns_All_Journals(int count)
        {
            var controller = GetController(DefaultData, SetUpRepository);

            var expected = Mapper.Map<List<Journal>, List<JournalViewModel>>(DefaultData);

            var result = controller.Index();
            
            var model = result.Should()
                  .BeViewResult()
                  .WithViewName("Index")
                  .ModelAs<List<JournalViewModel>>();


            model.Should()
                 .NotBeNull()
                 .And.HaveCount(count);

            foreach (var viewModel in model)
            {
                expected.Contains(viewModel).Should().BeTrue("all items in the view model must be equivalent to the items in the expected list");
            }
        }


        [Fact]
        public void Create_Returns_View_On_Get()
        {
            var controller = GetController(DefaultData, SetUpRepository);

            var result = controller.Create();

            result.Should().BeViewResult().WithViewName("Create");
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
            var controller = GetController(DefaultData, SetUpRepository, "POST");

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
                result.Should().BeViewResult("validation errors should appear to the user").WithViewName("Create");
            }
        }

        [Theory]
        [MemberData(nameof(ValidJournalViewModelsForCreate))]
        public void Create_Post_With_Valid_Args_Redirects_To_Index(JournalViewModel journal)
        {
            var controller = GetController(DefaultData, SetUpRepository, "POST");

            controller.ValidateViewModel(journal);
            var result = controller.Create(journal);

            controller.ModelState.IsValid.Should().BeTrue("ModelState should be valid");
            result.Should().BeRedirectToRouteResult().WithAction("Index");
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Delete_Id_With_Valid_Id_Shows_Confirmation_Page(int id)
        {
            var controller = GetController(DefaultData, SetUpRepository);

            var result = controller.Delete(id);

            result.Should()
                    .BeViewResult("the confirmation page should appear")
                    .WithViewName("Delete")
                    .Model.Should()
                    .NotBeNull()
                    .And.BeAssignableTo<JournalViewModel>();
        }

        [Theory]
        [InlineData(3, HttpStatusCode.NotFound)]
        [InlineData(4, HttpStatusCode.NotFound)]
        [InlineData(-1, HttpStatusCode.NotFound)]
        [InlineData(0, HttpStatusCode.NotFound)]
        [InlineData(int.MaxValue, HttpStatusCode.NotFound)]
        [InlineData(int.MinValue, HttpStatusCode.NotFound)]
        public void Delete_Id_With_Valid_Id_Shows_Error(int id, HttpStatusCode statusCode)
        {
            var controller = GetController(DefaultData, SetUpRepository);

            var result = controller.Delete(id);

            result.Should()
                  .BeAssignableTo<HttpStatusCodeResult>()
                  .Which.StatusCode.Should().Be((int)statusCode);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(1)]
        public void Delete_With_Valid_Model_Removes_Data(int id)
        {
            int count = int.MinValue;
            List<Journal> items = null;
            var controller = GetController(DefaultData,
                                           (m, r, u) =>
                                           {
                                               items = m;
                                               count = items.Count;
                                               SetUpRepository(m, r, u);
                                           });

            var journal = items.FirstOrDefault(i => i.Id == id);

            var viewModel = Mapper.Map<Journal, JournalViewModel>(journal);

            var result = controller.Delete(viewModel);

            result.Should().BeRedirectToRouteResult().WithAction("Index");

            items.Should().HaveCount(c => c < count);
            items.Should().NotContain(j => j.Id == journal.Id);
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Edit_Id_With_Valid_Id_Shows_Edit_Page(int id)
        {
            var controller = GetController(DefaultData, SetUpRepository);

            var result = controller.Edit(id);

            result.Should()
                    .BeViewResult("the edit page should appear")
                    .WithViewName("Edit")
                    .Model.Should()
                    .NotBeNull()
                    .And.BeAssignableTo<JournalUpdateViewModel>();
        }


        [Theory]
        [InlineData(3, HttpStatusCode.NotFound)]
        [InlineData(4, HttpStatusCode.NotFound)]
        [InlineData(-1, HttpStatusCode.NotFound)]
        [InlineData(0, HttpStatusCode.NotFound)]
        [InlineData(int.MaxValue, HttpStatusCode.NotFound)]
        [InlineData(int.MinValue, HttpStatusCode.NotFound)]
        public void Edit_Id_With_Valid_Id_Shows_Error(int id, HttpStatusCode statusCode)
        {
            var controller = GetController(DefaultData, SetUpRepository);

            var result = controller.Edit(id);

            result.Should()
                  .BeAssignableTo<HttpStatusCodeResult>()
                  .Which.StatusCode.Should().Be((int)statusCode);
        }


        public void Dispose()
        {
            Mapper.Reset();
        }

    }
}