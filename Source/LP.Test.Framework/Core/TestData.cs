using System.Collections.Generic;

namespace LP.Test.Framework.Core
{
    public abstract class TestData<TModel> : ITestData<TModel>
    {

        public virtual List<TModel> GetDefaultData()
        {
            return new List<TModel>();
        }
    }
}