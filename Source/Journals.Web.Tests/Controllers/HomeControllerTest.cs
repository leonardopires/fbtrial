using System.Web.Mvc;
using FluentAssertions;
using FluentAssertions.Mvc;
using Journals.Web.Controllers;
using Journals.Web.Tests.TestData;
using Xunit;

namespace Journals.Web.Tests.Controllers
{
    public class HomeControllerTest : MvcControllerTest<HomeController, object, StaticPagesTestData>
    {
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



    }
}