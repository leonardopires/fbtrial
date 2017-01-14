using AutoMapper;
using Journals.Model;
using Journals.Repository;
using Journals.Web.Controllers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Mvc;
using System.Web.Security;
using FluentAssertions;
using FluentAssertions.Mvc;
using Journals.Web.Tests.TestData;
using Microsoft.Data.OData.Query.SemanticAst;
using Newtonsoft.Json;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit;

namespace Journals.Web.Tests.Controllers
{
    public class PublisherDataControllerTest : DataControllerTest<PublisherController, Journal, IJournalRepository, JournalTestData>
    {

        public PublisherDataControllerTest()
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


        public static IEnumerable<object[]> InvalidJournalViewModels => Data.GetInvalidJournalViewModels();

        public static IEnumerable<object[]> ValidJournalViewModelsForCreate => Data.GetValidJournalViewModelsForCreate();
        public static IEnumerable<object[]> InvalidIdsAndExpectedStatusCodes => Data.GetInvalidIdsAndExpectedStatusCodes();

        public static IEnumerable<object[]> ValidUpdatedJournals => Data.GetValidUpdatedJournals();

        public static IEnumerable<object[]> InvalidUpdatedJournals => Data.GetInvalidUpdatedJournals();

        public static List<Journal> DefaultData => Data.GetDefaultData();


        [Theory]
        [InlineData(2)]
        public void Index_Returns_All_Journals(int count)
        {
            var controller = GetController();

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
            var controller = GetController();

            var result = controller.Create();

            result.Should().BeViewResult().WithViewName("Create");
        }

        [Theory]
        [InlineData(1, "application/pdf")]
        [InlineData(2, "application/pdf")]
        public void GetFile_Returns_File(int fileId, string expectedContentType)
        {
            var controller = GetController();
            var result = controller.GetFile(fileId);

            result.Should().BeOfType<FileContentResult>().Which.ContentType.Should().Be(expectedContentType);
        }

        [Theory]
        [MemberData(nameof(InvalidIdsAndExpectedStatusCodes))]
        public void GetFile_Returns_StatusCode_OnErrors(int fileId, HttpStatusCode httpStatus)
        {
            var controller = GetController();
            var result = controller.GetFile(fileId);

            result.Should().BeAssignableTo<HttpStatusCodeResult>().Which.StatusCode.Should().Be((int)httpStatus);
        }

        [Theory]
        [MemberData(nameof(InvalidJournalViewModels))]
        public void Create_Post_With_Invalid_Args_Returns_StatusCode(JournalViewModel journal, HttpStatusCode statusCode)
        {
            var controller = GetController("POST");

            controller.ValidateModel(journal);
            var result = controller.Create(journal);


            if (statusCode != HttpStatusCode.OK)
            {
                controller.ModelState.IsValid.Should().BeTrue("ModelState should be valid: {0}", controller.ModelState.Dump());
                result.Should().BeAssignableTo<HttpStatusCodeResult>().Which.StatusCode.Should().Be((int) statusCode, "the error should have happened in the repository");
            }
            else
            {
                controller.ModelState.IsValid.Should().BeFalse("ModelState should be invalid: {0}", controller.ModelState.Dump());
                result.Should().NotBeOfType<HttpStatusCodeResult>("validation errors should be identified by ModelState");
                result.Should().BeViewResult("validation errors should appear to the user").WithViewName("Create");
            }
        }

        [Theory]
        [MemberData(nameof(ValidJournalViewModelsForCreate))]
        public void Create_Post_With_Valid_Args_Redirects_To_Index(JournalViewModel journal)
        {
            var controller = GetController("POST");

            controller.ValidateModel(journal);
            var result = controller.Create(journal);

            controller.ModelState.IsValid.Should().BeTrue("ModelState should be valid: {0}", controller.ModelState.Dump());
            result.Should().BeRedirectToRouteResult().WithAction("Index");
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Delete_Id_With_Valid_Id_Shows_Confirmation_Page(int id)
        {
            var controller = GetController();

            var result = controller.Delete(id);

            result.Should()
                    .BeViewResult("the confirmation page should appear")
                    .WithViewName("Delete")
                    .Model.Should()
                    .NotBeNull()
                    .And.BeAssignableTo<JournalViewModel>();
        }

        [Theory]
        [MemberData(nameof(InvalidIdsAndExpectedStatusCodes))]
        public void Delete_Id_With_Valid_Id_Shows_Error(int id, HttpStatusCode statusCode)
        {
            var controller = GetController();

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
                                           (m, r) =>
                                           {
                                               items = m;
                                               count = items.Count;
                                               Data.SetUpRepository(m, r);
                                           });

            var journal = items.FirstOrDefault(i => i.Id == id);

            var viewModel = Mapper.Map<Journal, JournalViewModel>(journal);

            var result = controller.Delete(viewModel);

            result.Should().BeRedirectToRouteResult().WithAction("Index");

            items.Should().HaveCount(c => c < count);
            items.Should().NotContain(j => j.Id == journal.Id);
        }

        [Theory]
        [MemberData(nameof(InvalidJournalViewModels))]
        public void Delete_With_Invalid_Model_Does_Not_Remove_Data(JournalViewModel viewModel, HttpStatusCode statusCode)
        {
            int count = int.MinValue;
            List<Journal> items = null;
            var controller = GetController(DefaultData,
                                           (m, r) =>
                                           {
                                               items = m;
                                               count = items.Count;
                                               Data.SetUpRepository(m, r);
                                           });

            controller.ValidateModel(viewModel);
            var result = controller.Delete(viewModel);

            result.Should()
                  .BeAssignableTo<HttpStatusCodeResult>()
                  .Which.StatusCode.Should().Be(404);

            items.Should().HaveCount(count);
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Edit_Id_With_Valid_Id_Shows_Edit_Page(int id)
        {
            var controller = GetController();

            var result = controller.Edit(id);

            result.Should()
                    .BeViewResult("the edit page should appear")
                    .WithViewName("Edit")
                    .Model.Should()
                    .NotBeNull()
                    .And.BeAssignableTo<JournalUpdateViewModel>();
        }


        [Theory]
        [MemberData(nameof(InvalidIdsAndExpectedStatusCodes))]
        public void Edit_Id_With_Valid_Id_Shows_Error(int id, HttpStatusCode statusCode)
        {
            var controller = GetController();

            var result = controller.Edit(id);

            result.Should()
                  .BeAssignableTo<HttpStatusCodeResult>()
                  .Which.StatusCode.Should().Be((int)statusCode);
        }


        [Theory]
        [MemberData(nameof(ValidUpdatedJournals))]
        public void Edit_With_Valid_Model_Edits_Data(JournalUpdateViewModel viewModel)
        {
            int count = int.MinValue;
            List<Journal> items = null;
            var controller = GetController(DefaultData,
                                           (m, r) =>
                                           {
                                               items = m;
                                               count = items.Count;
                                               Data.SetUpRepository(m, r);
                                           }, "POST");

            controller.ValidateModel(viewModel);
            var result = controller.Edit(viewModel);

            items.Should().HaveCount(count, "nothing should be deleted nor inserted in an update operation");

            items.Should().HaveCount(count);

            var edited = items.Find(j => j.Id == viewModel.Id);

            controller.ModelState.IsValid.Should().BeTrue("ModelState should be valid: {0}", controller.ModelState.Dump());

            edited.Should().NotBeNull();

            edited.Id.Should().Be(viewModel.Id);
            edited.FileName.Should().Be(viewModel.FileName);
            edited.Content.Should().NotBeNull().And.Match(c => c.SequenceEqual(viewModel.Content));
            edited.ContentType.Should().Be(viewModel.ContentType);

            edited.Title.Should().Be(viewModel.Title);
            edited.Description.Should().Be(viewModel.Description);
            edited.UserId.Should().Be(viewModel.UserId);


            result.Should().BeRedirectToRouteResult().WithAction("Index");



        }



        [Theory]
        [MemberData(nameof(InvalidUpdatedJournals))]
        public void Edit_With_Invalid_Model_Does_Not_Change_Data(JournalUpdateViewModel viewModel, HttpStatusCode expectedStatusCode)
        {
            int count = int.MinValue;
            List<Journal> items = null;
            var controller = GetController(DefaultData,
                                           (m, r) =>
                                           {
                                               items = m;
                                               count = items.Count;
                                               Data.SetUpRepository(m, r);
                                           });

            var original = items.Find(i => i.Id == viewModel.Id);
            var originalViewModel = Mapper.Map<Journal, JournalUpdateViewModel>(original);

            controller.ValidateModel(viewModel);
            var result = controller.Edit(viewModel);

            if (expectedStatusCode == HttpStatusCode.OK)
            {
                items.Should().HaveCount(count, "nothing should be deleted nor inserted in an update operation");

                var edited = items.Find(j => j.Id == viewModel.Id);

                edited.Id.Should().Be(originalViewModel.Id);

                edited.FileName.Should().Be(originalViewModel.FileName);

                edited.Content.Should().NotBeNull().And.Match(c => c.SequenceEqual(originalViewModel.Content));

                edited.ContentType.Should().Be(originalViewModel.ContentType);

                edited.Title.Should().Be(originalViewModel.Title);

                edited.Description.Should().Be(originalViewModel.Description);

                edited.UserId.Should().Be(originalViewModel.UserId);


                result.Should().BeViewResult("validation error should appear to the user")
                      .WithViewName("Edit")
                      .Model.Should().Be(viewModel);

            }
            else
            {
                result.Should()
                      .BeAssignableTo<HttpStatusCodeResult>()
                      .Which.StatusCode.Should()
                      .Be((int) expectedStatusCode);
            }

        }

        public void Dispose()
        {
            Mapper.Reset();
        }

    }
}