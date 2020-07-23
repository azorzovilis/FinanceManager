namespace FinanceManagerAPI.DAL
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading.Tasks;
    using Microsoft.EntityFrameworkCore;
    using Microsoft.EntityFrameworkCore.Metadata;

    public class DataRepository<T> : IDataRepository<T> where T : class
    {
        private readonly DataContext _context;
        private readonly DbSet<T> _dbSet;
        private IProperty KeyProperty => _context.Model.FindEntityType(typeof(T)).FindPrimaryKey().Properties[0];
        
        public DataRepository(DataContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public IQueryable<T> Get(
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            Expression<Func<T, bool>> filter = null,
            params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            if (filter != null)
            {
                query = query.Where(filter);
            }

            query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

            return orderBy != null 
                ? orderBy(query)
                : query;
        }

        public async Task<T> GetById(int id, params Expression<Func<T, object>>[] includeProperties)
        {
            IQueryable<T> query = _dbSet;

            query = includeProperties.Aggregate(query, (current, includeProperty) => current.Include(includeProperty));

            return await query.FirstOrDefaultAsync(e => EF.Property<object>(e, KeyProperty.Name).Equals(id));
        }

        public async Task<T> GetByProperty(string propertyName, string propertyValue)
        {
            IQueryable<T> query = _dbSet;
            return await query.FirstOrDefaultAsync(e => EF.Property<object>(e, propertyName).Equals(propertyValue));
        }

        public async Task<bool> EntityExists(int id)
        {
            return await _dbSet.AnyAsync(e => EF.Property<object>(e, KeyProperty.Name).Equals(id));
        }

        public async Task<bool> EntityExists(string propertyName, string propertyValue)
        {
            return await _dbSet.AnyAsync(e => EF.Property<object>(e, propertyName).Equals(propertyValue));
        }

        public async Task Add(T entity)
        {
            await _dbSet.AddAsync(entity);
        }

        public void Update(T entity)
        {
            _dbSet.Update(entity);
            _context.Entry(entity).State = EntityState.Modified;
        }

        public void Delete(T entity)
        {
            _dbSet.Remove(entity);
        }

        public async Task<T> SaveAsync(T entity)
        {
            await _context.SaveChangesAsync();
            return entity;
        }
    }
}