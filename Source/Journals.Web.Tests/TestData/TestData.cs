using System.Collections.Generic;
using Telerik.JustMock;

namespace Journals.Web.Tests.TestData
{
    public abstract class TestData<TModel> : ITestData<TModel>
    {

        public virtual List<TModel> GetDefaultData()
        {
            return new List<TModel>();
        }
    }
}