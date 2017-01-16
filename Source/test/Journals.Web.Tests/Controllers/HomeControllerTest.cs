using System;
using Autofac;
using FluentAssertions;
using Journals.Web.Controllers;
using Journals.Web.Tests.Framework;
using Journals.Web.Tests.TestData;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Xunit.Abstractions;

namespace Journals.Web.Tests.Controllers
{
    public class HomeControllerTest : MvcControllerTest<HomeController, object, StaticPagesTestData>
    {

        public HomeControllerTest(ITestOutputHelper output) : base(output)
        {
        }

        /// <summary>
        /// Tests opening simple pages from the <see cref="HomeController"/>.
        /// </summary>
        /// <param name="action">The action.</param>
        /// <param name="message">The message.</param>
        [Theory]
        [MemberData(nameof(GetDataMember), nameof(Data.HomeControllerActions))]

        public void Shows_View(string action, string message)
        {
            var controller = GetController();
            var result = controller.InvokeAction<ActionResult>(action);

            var viewResult = result.Should().BeAssignableTo<ViewResult>().Which;
            viewResult.ViewData.Should().Contain("Message", message);
            viewResult.ViewName.Should().BeOneOf(null, string.Empty, action);
        }

        [Fact]
        public void Tests_Default_Data()
        {
            Data.GetDefaultData().Should().BeEmpty("there should be no default data for static pages");
        }

        [Fact]
        public void Echo_Must_Return_Value()
        {
            var guid = Guid.NewGuid();
            var controller = GetController();

            var result = controller.Echo(guid.ToString());

            result.Should().BeAssignableTo<JsonResult>().Which.Value.As<string>().Should().Contain(guid.ToString());
        }




    }
}