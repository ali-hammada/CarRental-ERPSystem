using ApplicationCore.Interfaces;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InFrastructure.Repositories
{
  public class GenericRepository<T>:IGenericRepository<T> where T : class
  {
    protected readonly AppDbContext _context;
    protected readonly DbSet<T> _dbSet;

    public GenericRepository(AppDbContext context)
    {
      _context=context;
      _dbSet=_context.Set<T>();
    }
    public Task AddAsync(T Entity)
    {
      _dbSet.Add(Entity);
      return Task.CompletedTask;
    }
    public void Delete(T Entity)
    {
      _dbSet.Remove(Entity);

    }
    public IQueryable<T> GetAll()
    {
      return _dbSet.AsQueryable();
    }

    public async Task<T?> GetByIdAsync(int id)
    {
      return await _dbSet.FindAsync(id);

    }
    public void Update(T Entity)
    {
      _dbSet.Update(Entity);

    }
  }
}
