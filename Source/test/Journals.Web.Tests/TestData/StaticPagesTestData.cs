using System;
using System.Collections.Generic;
using Journals.Web.Tests.Framework;
using LP.Test.Framework.Core;

namespace Journals.Web.Tests.TestData
{
    public class StaticPagesTestData : TestData<object>
    {
        public IEnumerable<object[]> HomeControllerActions =>
            Data(
            
                Item("About", "Your app description page."),
                Item("Contact", "Your contact page.")
            );


    }
}