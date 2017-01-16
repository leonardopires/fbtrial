using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;

using Journals.Web.Controllers;
using Microsoft.AspNetCore.Mvc;
using Xunit;
using Xunit.Abstractions;

namespace Journals.Web.Tests.Controllers
{
    public class ErrorControllerTest: MvcControllerTest<ErrorController>
    {

        public ErrorControllerTest(ITestOutputHelper output) : base(output)
        {
        }

        [Fact]
        public void RequestLengthExceeded_Shows_View()
        {
            var controller= GetController();
            var result = controller.RequestLengthExceeded();

            var errorInfo = result.Should().BeAssignableTo<BadRequestObjectResult>();
        }
    }
}
