using System;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Telerik.JustMock;

namespace Journals.Web.Tests.Controllers
{


    /// <summary>
    /// Implements the base functionality for a test that exercises an ASP.NET MVC controller
    /// </summary>
    /// <typeparam name="TController">The type of the controller to be tested.</typeparam>
    public abstract class MvcControllerTest<TController> : IDisposable
        where TController : Controller
    {

        /// <summary>
        /// Gets the controller instance for this test.
        /// </summary>
        /// <param name="httpMethod">The HTTP method sent to the controller.</param>
        /// <returns><see cref="TController"/></returns>
        protected virtual TController GetController(string httpMethod = "GET")
        {
            var controller = CreateControllerInstance();
            var mockHttpContext = Mock.Create<HttpContextBase>();
            var mockRequest = Mock.Create<HttpRequestBase>();

            Mock.Arrange(() => mockHttpContext.Request).Returns(mockRequest);
            Mock.Arrange(() => mockRequest.HttpMethod).Returns(httpMethod);

            controller.ControllerContext = new ControllerContext(mockHttpContext, new RouteData(), controller);

            return controller;
        }

        /// <summary>
        /// Creates the controller instance to be used by the test.
        /// </summary>
        /// <returns><see cref="TController" /></returns>
        protected virtual TController CreateControllerInstance()
        {
            return Activator.CreateInstance<TController>();
        }

        /// <summary>
        /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases unmanaged and - optionally - managed resources.
        /// </summary>
        /// <param name="disposing"><c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only unmanaged resources.</param>
        protected virtual void Dispose(bool disposing)
        {
        }

    }


    /// <summary>
    /// Implements the base functionality for a test that exercises an ASP.NET MVC controller
    /// </summary>
    /// <typeparam name="TController">The type of the controller to be tested.</typeparam>
    /// <typeparam name="TModel">The type of the model the controller uses.</typeparam>

    public abstract class MvcControllerTest<TController, TModel> : MvcControllerTest<TController>
        where TController : Controller
        where TModel : class, new()
    {

    }

    /// <summary>
    /// Implements the base functionality for a test that exercises an ASP.NET MVC controller
    /// </summary>
    /// <typeparam name="TController">The type of the controller to be tested.</typeparam>
    /// <typeparam name="TModel">The type of the model the controller uses.</typeparam>
    /// <typeparam name="TTestData">The type of the test data provider the test uses.</typeparam>

    public abstract class MvcControllerTest<TController, TModel, TTestData> : MvcControllerTest<TController, TModel>
        where TController : Controller
        where TModel : class, new()
        where TTestData : class, new()
    {
        private static readonly Lazy<TTestData> _data = new Lazy<TTestData>(() => new TTestData());

        public static TTestData Data => _data.Value;

    }
}