using System.Collections.Generic;

namespace LP.Test.Framework.Core
{

    public interface ITestData<TModel>
    {
        List<TModel> GetDefaultData();
    }

}