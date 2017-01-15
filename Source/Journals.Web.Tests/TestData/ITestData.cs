using System.Collections.Generic;

namespace Journals.Web.Tests.TestData
{

    public interface ITestData<TModel>
    {
        List<TModel> GetDefaultData();
    }

}