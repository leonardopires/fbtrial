using System;
using Journals.Model;
using Microsoft.EntityFrameworkCore;

namespace Journals.Repository
{
    public class OperationContext<TDataContext> : IDisposable
        where TDataContext : DbContext, IDisposedTracker
    {

        private readonly TDataContext dataContext;

        public TDataContext Data => (dataContext != null && !dataContext.IsDisposed) ? dataContext : null;

        public OperationStatus Result { get; set;  } = new OperationStatus();

        

        public OperationContext(Func<TDataContext> contextFactory)
        {
            if (contextFactory == null)
            {
                throw new ArgumentNullException(nameof(contextFactory));
            }

            dataContext = contextFactory();
        }

        public void Dispose()
        {
        }

    }
}