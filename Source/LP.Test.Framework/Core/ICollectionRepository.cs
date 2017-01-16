using System.Collections.Generic;

namespace LP.Test.Framework.Core
{
    public interface ICollectionRepository<T>
    {
        ICollection<T> GetItems();

    }
}