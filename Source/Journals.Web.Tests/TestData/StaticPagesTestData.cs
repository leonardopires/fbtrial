using System;
using System.Collections.Generic;
using Journals.Web.Tests.Framework;
using LP.Test.Framework.Core;

namespace Journals.Web.Tests.TestData
{
    public class StaticPagesTestData : TestData<object>
    {
        public IEnumerable<object[]> HomeControllerActions =>
            new List<object[]>
            {
                new object[] { "Index", "Open Journal Publishers"},
                new object[] { "About", "Your app description page."},
                new object[] { "Contact", "Your contact page."}
            };


    }
}