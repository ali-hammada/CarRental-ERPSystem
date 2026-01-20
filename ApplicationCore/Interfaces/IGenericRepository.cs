namespace ApplicationCore.Interfaces
{
  public interface IGenericRepository<T> where T : class
  {
    IQueryable<T> GetAll();
    Task<T> GetByIdAsync(int id);
    Task AddAsync(T Entity);
    void Update(T Entity);
    void Delete(T Entity);
  }
}
