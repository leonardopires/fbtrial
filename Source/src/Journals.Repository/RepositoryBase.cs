using Journals.Model;
using System;
using System.Data.Entity;
using System.Data.Entity.Validation;
using System.Linq;
using System.Linq.Expressions;

namespace Journals.Repository
{
    public class RepositoryBase<TDataContext> : IDisposable where TDataContext : DbContext, IDisposedTracker
    {

        private readonly Func<TDataContext> contextFactory;

        private TDataContext _DataContext;

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

                    AllowSerialization = true;
                }
                return _DataContext;
            }
        }

        protected virtual bool AllowSerialization
        {
            get
            {
                return _DataContext.Configuration.ProxyCreationEnabled;
            }
            set
            {
                _DataContext.Configuration.ProxyCreationEnabled = !value;
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

        protected virtual IDbSet<TEntity> Table<TEntity>() where TEntity : class
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
                try
                {

                    foreach (var operation in operations)
                    {
                        operation?.Invoke(operationContext);
                    }
                }
                catch (Exception e)
                {
                    OperationStatus.CreateFromException(e.Message, e);
                }
                return operationContext.Result;
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
                       context.Data.Entry(entity).State = EntityState.Added;
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
                _DataContext.IsDisposed = true;
            }
        }
    }
}