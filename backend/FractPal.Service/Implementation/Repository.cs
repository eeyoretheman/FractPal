namespace FractPal.Data;

using System.Linq.Expressions;

/// <summary>
/// Generic repository abstraction providing standard data access operations
/// for any entity type <typeparamref name="T"/>.
/// </summary>
/// <typeparam name="T">The entity type managed by this repository.</typeparam>
public interface IRepository<T> where T : class
{
    /// <summary>Retrieves an entity by its primary key.</summary>
    /// <param name="id">The string representation of the primary key.</param>
    /// <returns>The matching entity, or <c>null</c> if not found.</returns>
    public Task<T?> GetByIdAsync(string id);

    /// <summary>Retrieves all entities of type <typeparamref name="T"/>.</summary>
    /// <returns>An enumerable of all entities.</returns>
    public Task<IEnumerable<T>> GetAllAsync();

    /// <summary>Retrieves all entities matching the given predicate.</summary>
    /// <param name="predicate">A filter expression applied to the entity set.</param>
    /// <returns>An enumerable of matching entities.</returns>
    public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);

    /// <summary>Persists a new entity to the data store.</summary>
    /// <param name="entity">The entity to add.</param>
    /// <returns>The added entity, potentially including database-generated values such as IDs.</returns>
    public Task<T> AddAsync(T entity);

    /// <summary>Updates an existing entity in the data store.</summary>
    /// <param name="entity">The entity with updated values.</param>
    /// <returns>The updated entity.</returns>
    public Task<T> UpdateAsync(T entity);

    /// <summary>Removes an entity from the data store.</summary>
    /// <param name="entity">The entity to delete.</param>
    public Task DeleteAsync(T entity);

    /// <summary>
    /// Counts entities, optionally filtered by a predicate.
    /// </summary>
    /// <param name="predicate">An optional filter expression. When <c>null</c>, all entities are counted.</param>
    /// <returns>The number of matching entities.</returns>
    public Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
}

/// <summary>
/// Entity Framework Core implementation of <see cref="IRepository{T}"/>.
/// </summary>
/// <typeparam name="T">The entity type managed by this repository.</typeparam>
public class Repository<T>(ApplicationDbContext context) : IRepository<T> where T : class
{
    /// <summary>The underlying EF Core database context.</summary>
    protected ApplicationDbContext Context => context;

    /// <inheritdoc/>
    public virtual async Task<T?> GetByIdAsync(string id) => await this.Context.Set<T>().FindAsync(id);

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<T>> GetAllAsync() => await Task.FromResult(this.Context.Set<T>().ToList());

    /// <inheritdoc/>
    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate) => await Task.FromResult(this.Context.Set<T>().Where(predicate).ToList());

    /// <inheritdoc/>
    public virtual async Task<T> AddAsync(T entity)
    {
        await this.Context.Set<T>().AddAsync(entity);
        await this.Context.SaveChangesAsync();
        return entity;
    }

    /// <inheritdoc/>
    public virtual async Task<T> UpdateAsync(T entity)
    {
        this.Context.Set<T>().Update(entity);
        await this.Context.SaveChangesAsync();
        return entity;
    }

    /// <inheritdoc/>
    public virtual async Task DeleteAsync(T entity)
    {
        this.Context.Set<T>().Remove(entity);
        await this.Context.SaveChangesAsync();
    }

    /// <inheritdoc/>
    public virtual async Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null)
    {
        if (predicate == null)
        {
            return await Task.FromResult(this.Context.Set<T>().Count());
        }
        return await Task.FromResult(this.Context.Set<T>().Count(predicate));
    }
}
