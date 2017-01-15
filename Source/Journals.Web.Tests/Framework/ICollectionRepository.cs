using System.Collections.Generic;

namespace Journals.Web.Tests.Framework
{
    public interface ICollectionRepository<T>
    {
        ICollection<T> GetItems();

    }
}