using FluentAssertions;
using FluentAssertions.Mvc;
using Journals.Web.Controllers;
using Xunit;

namespace Journals.Web.Tests.Controllers
{
    public class HomeControllerTest : MvcControllerTest<HomeController>
    {
        [Fact]
        public void Index_Shows_View()
        {
            var controller = GetController();
            var result = controller.Index();

            result.Should().BeViewResult().WithDefaultViewName().WithViewData("Message", "Open Journal Publishers");
        }

        [Fact]
        public void About_Shows_View()
        {
            var controller = GetController();
            var result = controller.About();

            result.Should().BeViewResult().WithDefaultViewName().WithViewData("Message", "Your app description page.");
        }


        [Fact]
        public void Contact_Shows_View()
        {
            var controller = GetController();
            var result = controller.Contact();

            result.Should().BeViewResult().WithDefaultViewName().WithViewData("Message", "Your contact page.");
        }



        [Theory]
        [InlineData("Index", "Open Journal Publishers")]
        [InlineData("About", "Your app description page.")]
        [InlineData("Contact", "Your contact page.")]
        public void Shows_View(string action, string message)
        {
            var controller = GetController();
            var result = controller.ActionInvoker.InvokeAction(controller.ControllerContext, action);
        }



    }
}