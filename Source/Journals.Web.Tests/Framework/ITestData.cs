using System.Collections.Generic;

namespace Journals.Web.Tests.Framework
{

    public interface ITestData<TModel>
    {
        List<TModel> GetDefaultData();
    }

}