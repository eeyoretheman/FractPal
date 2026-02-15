namespace FractPal.Data;

using System.Linq.Expressions;

public interface IRepository<T> where T : class
{
    public Task<T?> GetByIdAsync(string id);
    public Task<IEnumerable<T>> GetAllAsync();
    public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    public Task<T> AddAsync(T entity);
    public Task<T> UpdateAsync(T entity);
    public Task DeleteAsync(T entity);
    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}

public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : class
{
    protected ApplicationDbContext Context => context;

    public virtual async Task<T?> GetByIdAsync(string id) => await this.Context.Set<T>().FindAsync(id);

    public virtual async Task<IEnumerable<T>> GetAllAsync() => await Task.FromResult(this.Context.Set<T>().ToList());

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => await Task.FromResult(this.Context.Set<T>().Where(predicate).ToList());

    public virtual async Task<T> AddAsync(T entity)
    {
        await this.Context.Set<T>().AddAsync(entity);
        await this.Context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        this.Context.Set<T>().Update(entity);
        await this.Context.SaveChangesAsync();
        return entity;
    }

    public virtual async Task DeleteAsync(T entity)
    {
        this.Context.Set<T>().Remove(entity);
        await this.Context.SaveChangesAsync();
    }

    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return await Task.FromResult(this.Context.Set<T>().Count());
        }
        return await Task.FromResult(this.Context.Set<T>().Count(predicate));
    }
}
