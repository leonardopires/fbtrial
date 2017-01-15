using System;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using FluentAssertions.Mvc;
using Journals.Web.Controllers;
using Journals.Web.Tests.Framework;
using Journals.Web.Tests.TestData;
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

            result.Should().BeViewResult().WithDefaultViewName().WithViewData("Message", message);
        }

        [Fact]
        public void Tests_Default_Data()
        {
            Data.GetDefaultData().Should().BeEmpty("there should be no default data for static pages");
        }



    }
}