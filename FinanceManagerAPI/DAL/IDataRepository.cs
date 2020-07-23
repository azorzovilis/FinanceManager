namespace FinanceManagerAPI.DAL
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;

    public interface IDataRepository<T> where T : class
    {
        Task Add(T entity);

        void Update(T entity);

        void Delete(T entity);

        IQueryable<T> Get(
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Expression<Func<T, bool>> filter = null,
            params Expression<Func<T, object>>[] includeProperties);

        Task<T> GetById(int id, params Expression<Func<T, object>>[] includeProperties);

        Task<T> GetByProperty(string propertyName, string propertyValue);

        Task<bool> EntityExists(int id);

        Task<bool> EntityExists(string propertyName, string propertyValue);

        Task<T> SaveAsync(T entity);
    }
}