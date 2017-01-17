using System;
using Journals.Model;
using Microsoft.EntityFrameworkCore;

namespace Journals.Repository
{
    public class OperationContext<TDataContext> : IDisposable
        where TDataContext : DbContext, IDisposedTracker
    {

        private readonly Func<TDataContext> contextFactory;

        private readonly TDataContext dataContext;

        public TDataContext Data => (dataContext != null && !dataContext.IsDisposed) ? dataContext : contextFactory();

        public OperationStatus Result { get; set;  } = new OperationStatus();

        

        public OperationContext(Func<TDataContext> contextFactory)
        {
            this.contextFactory = contextFactory;
            if (contextFactory == null)
            {
                throw new ArgumentNullException(nameof(contextFactory));
            }

            dataContext = contextFactory();
        }

        public void Dispose()
        {
            dataContext.Dispose();
        }

    }
}