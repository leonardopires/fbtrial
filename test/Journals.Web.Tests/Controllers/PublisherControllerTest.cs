using AutoMapper;
using Journals.Model;
using Journals.Repository;
using Journals.Web.Controllers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Journals.Services;
using Journals.Web.Tests.Framework;
using Journals.Web.Tests.TestData;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Xunit;
using Xunit.Abstractions;

namespace Journals.Web.Tests.Controllers
{
    public class PublisherControllerTest : MvcControllerTest<PublisherController, Journal, JournalTestData>
    {
        private IMapper Mapper { get; }

        public PublisherControllerTest(ITestOutputHelper output) : base(output)
        {
            Mapper = Container.Resolve<IMapper>();
        }

        protected override void InitializeContainer(ContainerBuilder builder)
        {
            builder.RegisterModule<MocksModule>();

            builder.Register(
                r =>
                {
                    MapperConfiguration config = new MapperConfiguration(
                        c =>
                        {
                            c.CreateMap<Journal, JournalViewModel>();
                            c.CreateMap<JournalViewModel, Journal>();

                            c.CreateMap<Journal, JournalUpdateViewModel>();
                            c.CreateMap<JournalUpdateViewModel, Journal>();

                            c.CreateMap<Journal, SubscriptionViewModel>();
                            c.CreateMap<SubscriptionViewModel, Journal>();
                        });
                    return config.CreateMapper();
                }).As<IMapper>();

            base.InitializeContainer(builder);
        }


        public static List<Journal> DefaultData => Data.GetDefaultData();


        [Theory]
        [InlineData(2)]
        public void Index_Returns_All_Journals(int count)
        {
            var controller = GetController();

            var expected = Mapper.Map<List<Journal>, List<JournalViewModel>>(DefaultData);

            var result = controller.Index();

            var viewResult = result.Should().BeAssignableTo<ViewResult>().Which;
            viewResult.ViewName.Should().Be(nameof(controller.Index));

            var model = viewResult.Model.As<List<JournalViewModel>>();


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

            var viewResult = result.Should().BeAssignableTo<ViewResult>().Which;
            viewResult.ViewName.Should().Be(nameof(controller.Create));
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
        [MemberData(nameof(GetDataMember), nameof(Data.GetFileIdsAndExpectedStatusCodes))]
        public void GetFile_Returns_StatusCode_OnErrors(int fileId, HttpStatusCode httpStatus)
        {
            var controller = GetController();
            var result = controller.GetFile(fileId);

            if (httpStatus == HttpStatusCode.OK)
            {
                var fileResult = result.Should().BeAssignableTo<FileContentResult>().Which;

                fileResult.ContentType.Should().Be("application/pdf");
                fileResult.FileContents.Should().NotBeNull();
                fileResult.FileContents.Length.Should().BeGreaterOrEqualTo(0);
                fileResult.FileDownloadName.Should().NotBeNullOrWhiteSpace();
            }
            else
            {
                result.Should().BeAssignableTo<StatusCodeResult>().Which.StatusCode.Should().Be((int) httpStatus);
            }
        }

        [Theory]
        [MemberData(nameof(GetDataMember), nameof(Data.GetInvalidJournalViewModels))]
        public void Create_Post_With_Invalid_Args_Returns_StatusCode(JournalViewModel journal, HttpStatusCode statusCode)
        {
            var controller = GetController("POST");

            controller.ValidateModel(journal);
            var result = controller.Create(journal);

            Logger.Debug("{@journal}", journal);

            if (statusCode != HttpStatusCode.OK)
            {
                controller.ModelState.IsValid.Should().BeTrue("ModelState should be valid: ID: {1} - {0}", controller.ModelState.Dump(), journal.Id);
                result.Should().BeAssignableTo<StatusCodeResult>().Which.StatusCode.Should().Be((int)statusCode, "the error should have happened in the repository");
            }
            else
            {
                controller.ModelState.IsValid.Should().BeFalse("ModelState should be invalid: ID: {1} - {0}", controller.ModelState.Dump(), journal.Id);
                result.Should().NotBeOfType<StatusCodeResult>("validation errors should be identified by ModelState");

                var viewResult = result.Should().BeAssignableTo<ViewResult>("validation errors should appear to the user").Which;
                viewResult.ViewName.Should().Be(nameof(controller.Create));
            }
        }

        [Theory]
        [MemberData(nameof(GetDataMember), nameof(Data.GetValidJournalViewModelsForCreate))]
        public void Create_Post_With_Valid_Args_Redirects_To_Index(JournalViewModel journal)
        {
            var controller = GetController("POST");

            controller.ValidateModel(journal);
            var result = controller.Create(journal);

            controller.ModelState.IsValid.Should().BeTrue("ModelState should be valid: {0}", controller.ModelState.Dump());

            result.Should()
                  .BeAssignableTo<RedirectToActionResult>()
                  .Which.ActionName.Should()
                  .Be(nameof(controller.Index));
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Delete_Id_With_Valid_Id_Shows_Confirmation_Page(int id)
        {
            var controller = GetController();

            var result = controller.Delete(id);

            var viewResult = result.Should()
                                   .BeAssignableTo<ViewResult>("the confirmation page should appear").Which;

            viewResult.ViewName.Should().Be(nameof(controller.Delete));

            viewResult.Model.Should()
                    .NotBeNull()
                    .And.BeAssignableTo<JournalViewModel>();
        }

        [Theory]
        [MemberData(nameof(GetDataMember), nameof(Data.GetInvalidIdsAndExpectedStatusCodes))]
        public void Delete_Id_With_Valid_Id_Shows_Error(int id, HttpStatusCode statusCode)
        {
            var controller = GetController();

            var result = controller.Delete(id);

            result.Should()
                  .BeAssignableTo<StatusCodeResult>()
                  .Which.StatusCode.Should().Be((int)statusCode);
        }

        [Theory]
        [InlineData(2)]
        [InlineData(1)]
        public void Delete_With_Valid_Model_Removes_Data(int id)
        {
            var journalRepository = Container.Resolve<IJournalRepository>();

            var journal = journalRepository.GetJournalById(id);

            var items = journalRepository.GetAllJournals(journal.UserId);
            int count = items.Count;

            var controller = GetController();


            var viewModel = Mapper.Map<Journal, JournalViewModel>(journal);

            var result = controller.Delete(viewModel);

            result.Should().BeAssignableTo<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(controller.Index));

            items = journalRepository.GetAllJournals(journal.UserId);
            items.Should().HaveCount(c => c < count, "count should be less than {0}", count);
            items.Should().NotContain(j => j.Id == journal.Id);
        }

        [Theory]
        [MemberData(nameof(GetDataMember), nameof(Data.GetInvalidJournalViewModels))]
        public void Delete_With_Invalid_Model_Does_Not_Remove_Data(JournalViewModel viewModel, HttpStatusCode statusCode)
        {
            var journalRepository = Container.Resolve<IJournalRepository>();

            int count = journalRepository.GetAllJournals(viewModel.UserId).Count;

            var controller = GetController();

            controller.ValidateModel(viewModel);
            var result = controller.Delete(viewModel);

            result.Should()
                  .BeAssignableTo<StatusCodeResult>()
                  .Which.StatusCode.Should().Be(404);

            journalRepository.GetAllJournals(viewModel.UserId).Should().HaveCount(count);
        }


        [Theory]
        [InlineData(1)]
        [InlineData(2)]
        public void Edit_Id_With_Valid_Id_Shows_Edit_Page(int id)
        {
            var controller = GetController();

            var result = controller.Edit(id);

            var viewResult = result.Should().BeAssignableTo<ViewResult>("the edit page should appear").Which;

            viewResult.ViewName.Should().Be(nameof(controller.Edit));

            viewResult.Model.Should()
                    .NotBeNull()
                    .And.BeAssignableTo<JournalUpdateViewModel>();
        }


        [Theory]
        [MemberData(nameof(GetDataMember), nameof(Data.GetInvalidIdsAndExpectedStatusCodes))]
        public void Edit_Id_With_Valid_Id_Shows_Error(int id, HttpStatusCode statusCode)
        {
            var controller = GetController();

            var result = controller.Edit(id);

            result.Should()
                  .BeAssignableTo<StatusCodeResult>()
                  .Which.StatusCode.Should().Be((int)statusCode);
        }


        [Theory]
        [MemberData(nameof(GetDataMember), nameof(Data.GetValidUpdatedJournals))]
        public void Edit_With_Valid_Model_Edits_Data(JournalUpdateViewModel viewModel, int count)
        {
            var controller = GetController("POST");

            var journalRepository = Container.Resolve<IJournalRepository>();


            controller.ValidateModel(viewModel);
            var result = controller.Edit(viewModel);

            var items = journalRepository.GetAllJournals(viewModel.UserId);

            items.Should().HaveCount(count, "nothing should be deleted nor inserted in an update operation");
            items.Should().HaveCount(count);

            var edited = journalRepository.GetJournalById(viewModel.Id);

            controller.ModelState.IsValid.Should().BeTrue("ModelState should be valid: {0}", controller.ModelState.Dump());

            edited.ShouldBeEquivalentTo(viewModel);

            result.Should().BeAssignableTo<RedirectToActionResult>().Which.ActionName.Should().Be(nameof(controller.Index));
        }


        [Theory]
        [MemberData(nameof(GetDataMember), nameof(Data.GetInvalidUpdatedJournals))]
        public void Edit_With_Invalid_Model_Does_Not_Change_Data(JournalUpdateViewModel viewModel, HttpStatusCode expectedStatusCode)
        {

            var journalRepository = Container.Resolve<IJournalRepository>();
            IList<Journal> items = journalRepository.GetAllJournals(viewModel.UserId);

            int count = items.Count;

            var controller = GetController();

            var original = journalRepository.GetJournalById(viewModel.Id);
            var originalViewModel = Mapper.Map<Journal, JournalUpdateViewModel>(original);

            controller.ValidateModel(viewModel);
            var result = controller.Edit(viewModel);

            if (expectedStatusCode == HttpStatusCode.OK)
            {
                items = journalRepository.GetAllJournals(originalViewModel.UserId);

                items.Should().HaveCount(count, "nothing should be deleted nor inserted in an update operation");

                var edited = journalRepository.GetJournalById(viewModel.Id);

                edited.ShouldBeEquivalentTo(originalViewModel);


                var viewResult = result.Should().BeAssignableTo<ViewResult>("validation error should appear to the user").Which;

                viewResult.ViewName.Should().Be(nameof(controller.Edit));
                viewResult.Model.Should().Be(viewModel);

            }
            else
            {
                var viewResult = result.Should().BeAssignableTo<ViewResult>("validation error should appear to the user").Which;

                viewResult.ViewName.Should().BeOneOf(nameof(controller.Edit), null);
                viewResult.Model.Should().Be(viewModel);
                viewResult.StatusCode.Should().Be((int)HttpStatusCode.InternalServerError);
            }

        }

        [Theory]
        [MemberData(nameof(GetDataMember), nameof(Data.GetFilesToUpload))]
        public async Task Upload_File(IFormFile file)
        {
            var controller = GetController();
            var result = await controller.PostUpload(file);

            if (result is StatusCodeResult)
            {
                Logger.Error("{@result}", result);
            }

            var okResult = result.Should().BeAssignableTo<ObjectResult>().Which;

            var status = okResult.Value.As<OperationStatus>();

            Logger.Debug("{@status}", status);

            status.Status.Should().BeTrue();
        }

    }
}