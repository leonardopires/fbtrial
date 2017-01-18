using Journals.Model;
using System;
using System.Linq;
using System.Linq.Expressions;
using LP.Framework;
using Microsoft.EntityFrameworkCore;

namespace Journals.Repository
{
    public class RepositoryBase<TDataContext> : IDisposable where TDataContext : DbContext, IDisposedTracker
    {

        private readonly Func<TDataContext> contextFactory;

        private TDataContext _DataContext;

        public bool ThrowExceptionOnError { get; set; } = true;

        public RepositoryBase(Func<TDataContext> contextFactory)
        {
            this.contextFactory = contextFactory;
        }

        protected virtual TDataContext DataContext
        {
            get
            {
                if (_DataContext == null || _DataContext.IsDisposed)
                {
                    _DataContext = contextFactory();
                }
                return _DataContext;
            }
        }

        protected virtual TEntity Get<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            if (predicate != null)
            {
                using (DataContext)
                {
                    return DataContext.Set<TEntity>().Where(predicate).FirstOrDefault();
                }
            }
            else
            {
                throw new ApplicationException("Predicate value must be passed to Get<T>.");
            }
        }

        protected virtual IQueryable<TEntity> GetMany<TEntity>(Expression<Func<TEntity, bool>> predicate) where TEntity : class
        {
            var coll = DataContext.Set<TEntity>();
            if (predicate != null)
            {
                return coll.Where(predicate);
            }
            return coll;
        }

        protected virtual IQueryable<TEntity> GetMany<TEntity, TKey>(Expression<Func<TEntity, bool>> predicate,
            Expression<Func<TEntity, TKey>> orderBy) where TEntity : class
        {
            return GetMany(predicate).OrderBy(orderBy);
        }

        protected virtual IQueryable<TEntity> GetMany<TEntity, TKey>(Expression<Func<TEntity, TKey>> orderBy) where TEntity : class
        {
            return Table<TEntity>().OrderBy(orderBy);
        }

        protected virtual IQueryable<TEntity> GetMany<TEntity>() where TEntity : class
        {
            return Table<TEntity>();
        }

        protected virtual DbSet<TEntity> Table<TEntity>() where TEntity : class
        {
            return DataContext.Set<TEntity>();
        }

        protected OperationStatus ExecuteStoreCommand(string cmdText, params object[] parameters)
        {            
            return ExecuteOperations(context => context.Data.Database.ExecuteSqlCommand(cmdText, parameters));
        }

        protected virtual OperationStatus ExecuteOperations(params Action<OperationContext<TDataContext>>[] operations)
        {
            using (var operationContext = new OperationContext<TDataContext>(contextFactory))
            {
                var operationContextResult = operationContext.Result;
                try
                {

                    foreach (var operation in operations)
                    {
                        operation?.Invoke(operationContext);
                    }
                    if (operationContextResult.RecordsAffected == 0
                        && !operationContextResult.Status)
                    {
                        operationContextResult.Message = $"No rows affected: {operationContextResult.Message}";
                    }
                }
                catch (Exception e)
                {
                    operationContext.Result = OperationStatus.CreateFromException(e.Message, e);

                    if (ThrowExceptionOnError)
                    {
                        throw;
                    }

                }
                return operationContextResult;
            }

        }

        protected virtual Action<OperationContext<TDataContext>> Update<TEntity>(TEntity entity) where TEntity : class
        {
            return (context) =>
            {
                context.Data.Entry(entity).State = EntityState.Modified;
            };
        }

        protected virtual Action<OperationContext<TDataContext>> Remove<TEntity>(TEntity entity) where TEntity : class
        {
            return (context) =>
            {
                context.Data.Set<TEntity>().Remove(entity);
                context.Data.Entry(entity).State = EntityState.Deleted;
            };
        }

        protected virtual Action<OperationContext<TDataContext>> Add<TEntity>(TEntity entity) where TEntity : class 
        {
            return (context) =>
                   {
                       Table<TEntity>().Add(entity);
                   };
        }

        protected virtual Action<OperationContext<TDataContext>> Save
        {
            get { return (context => context.Result.RecordsAffected = context.Data.SaveChanges()); }
        }


        public virtual void Dispose()
        {
            if (_DataContext != null && !_DataContext.IsDisposed)
            {
                _DataContext.Dispose();
            }
        }
    }

    public class OperationException : Exception
    {

        public OperationStatus Status { get; }

        public OperationException(OperationStatus status)
        {
            Status = status;
        }

        public override string ToString()
        {
            return Status.ToString();
        }

    }
}