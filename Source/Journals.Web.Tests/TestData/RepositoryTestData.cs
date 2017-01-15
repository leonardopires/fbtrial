using System.Collections.Generic;
using Telerik.JustMock;

namespace Journals.Web.Tests.TestData
{
    public abstract class RepositoryTestData<TModel, TRepository> : ITestData<TModel, TRepository>
    {

        public virtual List<TModel> GetDefaultData()
        {
            return new List<TModel>();
        }

        public abstract void SetUpRepository(List<TModel> models, TRepository modelRepository);

        public TRepository CreateRepository()
        {
            var repo = Mock.Create<TRepository>();

            SetUpRepository(GetDefaultData(), repo);

            return repo;
        }

    }
}