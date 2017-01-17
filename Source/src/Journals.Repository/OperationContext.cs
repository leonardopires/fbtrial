using System;
using System.Data.Entity;
using Journals.Model;

namespace Journals.Repository
{
    public class OperationContext<TDataContext> : IDisposable
        where TDataContext : DbContext, IDisposedTracker
    {

        private readonly TDataContext dataContext;

        public TDataContext Data => (dataContext != null && !dataContext.IsDisposed) ? dataContext : null;

        public OperationStatus Result { get; } = new OperationStatus();

        

        public OperationContext(Func<TDataContext> contextFactory)
        {
            if (contextFactory == null)
            {
                throw new ArgumentNullException(nameof(contextFactory));
            }

            dataContext = contextFactory();

            dataContext.Configuration.ProxyCreationEnabled = true;
        }

        public void Dispose()
        {
            if (dataContext != null && !dataContext.IsDisposed)
            {
                dataContext.Dispose();
            }
        }

    }
}