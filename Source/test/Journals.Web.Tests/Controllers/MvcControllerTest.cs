using System;
using System.Collections.Generic;
using Autofac;
using FluentAssertions.Common;
using LP.Test.Framework.Core;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Serilog.Extensions.Logging;
using Telerik.JustMock;
using Telerik.JustMock.Helpers;
using Xunit.Abstractions;

namespace Journals.Web.Tests.Controllers
{
    /// <summary>
    ///     Implements the base functionality for a test that exercises an ASP.NET MVC controller
    /// </summary>
    /// <typeparam name="TController">The type of the controller to be tested.</typeparam>
    public abstract class MvcControllerTest<TController> : TestBase
        where TController : Controller
    {

        protected MvcControllerTest(ITestOutputHelper output) : base(output)
        {
        }

        /// <summary>
        ///     Gets the controller instance for this test.
        /// </summary>
        /// <param name="httpMethod">The HTTP method sent to the controller.</param>
        /// <returns>
        ///     <see cref="TController" />
        /// </returns>
        protected virtual TController GetController(string httpMethod = "GET")
        {
            var controller = Container.Resolve<TController>();


            return controller;
        }

        /// <summary>
        ///     Initializes the container.
        /// </summary>
        /// <param name="builder">The builder.</param>
        protected override void InitializeContainer(ContainerBuilder builder)
        {
            builder.Register(
                c =>
                {
                    var mock = Mock.Create<IHostingEnvironment>();

                    mock.Arrange(e => e.EnvironmentName).Returns(EnvironmentName.Development);

                    return mock;
                }).As<IHostingEnvironment>();

            builder.Register(c => new LoggerFactory()).As<ILoggerFactory>();

            builder.Register(
                c =>
                {
                    var provider = new SerilogLoggerProvider(Logger);
                    c.Resolve<ILoggerFactory>().AddProvider(provider);
                    return provider;
                }).As<ILoggerProvider>();

            builder.Register(c => c.Resolve<ILoggerFactory>().CreateLogger<TController>()).As<ILogger<TController>>();

            builder.RegisterType<TController>();
        }

    }


    /// <summary>
    ///     Implements the base functionality for a test that exercises an ASP.NET MVC controller
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
    ///     Implements the base functionality for a test that exercises an ASP.NET MVC controller
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
        ///     Retrieves the value of a member from the Data class.
        /// </summary>
        /// <param name="memberName">Name of the member that contains the value be retrieved.</param>
        /// <returns>
        ///     <see cref="object" />
        /// </returns>
        public static IEnumerable<object[]> GetDataMember(string memberName)
        {
            IEnumerable<object[]> result = new object[][] {};

            var type = Data.GetType();

            var property = type.GetPropertyByName(memberName);

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
