using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using Autofac;
using Autofac.Core;
using Autofac.Integration.Mvc;
using FluentAssertions.Common;
using Journals.Web.Tests.Framework;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit.Abstractions;

namespace Journals.Web.Tests.Controllers
{
    /// <summary>
    /// Implements the base functionality for a test that exercises an ASP.NET MVC controller
    /// </summary>
    /// <typeparam name="TController">The type of the controller to be tested.</typeparam>
    public abstract class MvcControllerTest<TController> : TestBase
        where TController : Controller
    {

        protected MvcControllerTest(ITestOutputHelper output) : base(output)
        {
        }

        /// <summary>
        /// Gets the controller instance for this test.
        /// </summary>
        /// <param name="httpMethod">The HTTP method sent to the controller.</param>
        /// <returns><see cref="TController"/></returns>
        protected virtual TController GetController(string httpMethod = "GET")
        {
            var controller = Container.Resolve<TController>();

            controller.ControllerContext = new ControllerContext(Container.Resolve<HttpContextBase>(new NamedParameter("httpMethod", httpMethod)), new RouteData(), controller);

            var resolver = new AutofacDependencyResolver(Container.BeginLifetimeScope());
            DependencyResolver.SetResolver(resolver);

            return controller;
        }

        /// <summary>
        /// Initializes the container.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected override void InitializeContainer(ContainerBuilder builder)
        {
            builder.Register(
                       (c, p) =>
                       {
                           var mock = Mock.Create<HttpRequestBase>();
                           mock.Arrange(r => r.HttpMethod).Returns(p.Named<string>("httpMethod"));
                           return mock;
                       }).As<HttpRequestBase>();

            builder.Register(
                       (c, p) =>
                       {
                           var mock = Mock.Create<HttpContextBase>();
                           mock.Arrange(i => i.Request)
                               .Returns(
                                   c.Resolve<HttpRequestBase>(
                                       new NamedParameter("httpMethod", p.Named<string>("httpMethod"))));
                           return mock;
                       })
                .As<HttpContextBase>();                    
               

            builder.RegisterType<TController>();
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

        protected MvcControllerTest(ITestOutputHelper output) : base(output)
        {
        }

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
        where TTestData : class
    {        

        private static readonly Lazy<TTestData> _data = new Lazy<TTestData>(() => RootContainer.Resolve<TTestData>());

        protected MvcControllerTest(ITestOutputHelper output) : base(output)
        {
        }

        public static TTestData Data => _data.Value;

        /// <summary>
        /// Retrieves the value of a member from the Data class.
        /// </summary>
        /// <param name="memberName">Name of the member that contains the value be retrieved.</param>
        /// <returns>
        ///   <see cref="object" />
        /// </returns>
        public static IEnumerable<object[]> GetDataMember(string memberName)
        {
            IEnumerable<object[]> result = new object[][] { };

            var type = Data.GetType();

            var property = type.GetProperty(memberName, typeof(IEnumerable<object[]>));

            if (property != null)
            {
                result = (IEnumerable<object[]>) property.GetValue(Data);
            }
            else
            {
                var method = type.GetParameterlessMethod(memberName);

                if (method != null)
                {
                    result = (IEnumerable<object[]>) method.Invoke(Data, null);
                }
            }
            return result;
        }

 

    }
}