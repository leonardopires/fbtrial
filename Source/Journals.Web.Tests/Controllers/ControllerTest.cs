using System;
using System.Collections.Generic;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.Security;
using AutoMapper;
using Journals.Model;
using Journals.Repository;
using Telerik.JustMock;

namespace Journals.Web.Tests.Controllers
{
    public abstract class ControllerTest<TController, TModel, TRepository, TTestData> : IDisposable
        where TController : Controller
        where TModel : class, new()
        where TRepository : IDisposable
        where TTestData : class, new()
    {
        private static readonly Lazy<TTestData> _data = new Lazy<TTestData>(() => new TTestData());

        public static TTestData Data => _data.Value;

        protected virtual TController GetController(List<TModel> modelItems, Action<List<TModel>, TRepository, MembershipUser> setupAction, string httpMethod = "GET")
        {
            Mapper.CreateMap<Journal, JournalViewModel>();
            //Arrange
            var membershipRepository = Mock.Create<IStaticMembershipService>();
            var userMock = Mock.Create<MembershipUser>();
            Mock.Arrange(() => userMock.ProviderUserKey).Returns(1);
            Mock.Arrange(() => membershipRepository.GetUser()).Returns(userMock);

            var repository = Mock.Create<TRepository>();

            var itemsCopy = new List<TModel>(modelItems);

            setupAction?.Invoke(itemsCopy, repository, userMock);

            //Act
            TController controller = CreateControllerInstance(repository, membershipRepository);

            var mockHttpContext = Mock.Create<HttpContextBase>();
            var mockRequest = Mock.Create<HttpRequestBase>();

            Mock.Arrange(() => mockHttpContext.Request).Returns(mockRequest);
            Mock.Arrange(() => mockRequest.HttpMethod).Returns(httpMethod);

            controller.ControllerContext = new ControllerContext(mockHttpContext, new RouteData(), controller);

            return controller;
        }

        protected abstract TController CreateControllerInstance(TRepository repository, IStaticMembershipService membershipService);

        protected abstract void SetUpRepository(
            List<TModel> models,
            TRepository modelRepository,
            MembershipUser userMock);

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
        }

    }
}