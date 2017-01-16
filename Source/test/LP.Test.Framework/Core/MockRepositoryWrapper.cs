using System;
using System.Collections.Generic;
using Telerik.JustMock;

namespace LP.Test.Framework.Core
{
    public abstract class MockRepositoryWrapper<TItem, TRepository> : IMockWrapper<TRepository>, ICollectionRepository<TItem>
        where TRepository : IDisposable
    {

        protected TRepository mock;
        protected List<TItem> models;

        public MockRepositoryWrapper(ITestData<TItem> testData)
        {
            models = new List<TItem>(testData.GetDefaultData());

            mock = Mock.Create<TRepository>();
        }

        public abstract void ArrangeMock();

        public void Dispose()
        {
            mock.Dispose();
        }

        TRepository IMockWrapper<TRepository>.GetMock()
        {
            return mock;
        }

        ICollection<TItem> ICollectionRepository<TItem>.GetItems()
        {
            return models;
        }

    }
}