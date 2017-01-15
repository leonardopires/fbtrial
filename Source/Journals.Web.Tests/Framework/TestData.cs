using System.Collections.Generic;

namespace Journals.Web.Tests.Framework
{
    public abstract class TestData<TModel> : ITestData<TModel>
    {

        public virtual List<TModel> GetDefaultData()
        {
            return new List<TModel>();
        }
    }
}