﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Mvc;
using Autofac;
using FluentAssertions;
using FluentAssertions.Mvc;

using Journals.Web.Controllers;
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

            var errorInfo = result.Should().BeViewResult().WithDefaultViewName().Model.Should().BeAssignableTo<HandleErrorInfo>().Which;
            errorInfo.ActionName.Should().Be("RequestLengthExceeded");
            errorInfo.ControllerName.Should().Be("Error");
        }
    }
}
