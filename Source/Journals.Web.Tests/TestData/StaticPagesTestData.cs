using System;
using System.Collections.Generic;

namespace Journals.Web.Tests.TestData
{
    public class StaticPagesTestData : ITestData<object>
    {

        public List<object> GetDefaultData()
        {
            return null;
        }

        public IEnumerable<object[]> HomeControllerActions =>
            new List<object[]>
            {
                new object[] { "Index", "Open Journal Publishers"},
                new object[] { "About", "Your app description page."},
                new object[] { "Contact", "Your contact page."}
            };


    }
}