using System.Collections.Generic;

namespace Journals.Web.Tests.TestData
{

    public interface ITestData<TModel>
    {
        List<TModel> GetDefaultData();
    }

    public interface ITestData<TModel, TRepository> : ITestData<TModel>
    {
        void SetUpRepository(List<TModel> models, TRepository modelRepository);

        TRepository CreateRepository();

    }

}