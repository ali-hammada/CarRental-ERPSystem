using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Repositories;
using InFrastructure.Data;
using InFrastructure.Repositories;

namespace InFrastructure.UnitOfWork
{
  public class UnitOfWork:IUnitOfWork
  {
    private readonly AppDbContext _context;

    public UnitOfWork(AppDbContext context)
    {
      _context=context;

      Cars=new CarRepository(_context);
      Customer=new CustomerRepository(_context);
      RentalContracts=new GenericRepository<RentalContract>(_context);
      Payments=new GenericRepository<Payment>(_context);
      Employee=new GenericRepository<Employees>(_context);
    }

    public ICarRepository Cars { get; }

    public ICustomerRepository Customer { get; }

    public IGenericRepository<RentalContract> RentalContracts { get; }

    public IGenericRepository<Payment> Payments { get; }

    public IGenericRepository<Employees> Employee { get; private set; }



    public async Task BeginTransactionAsync()
    {
      await _context.Database.BeginTransactionAsync();
    }

    public async Task CommitAsync()
    {
      await _context.Database.CommitTransactionAsync();
    }

    public async Task RollbackAsync()
    {
      await _context.Database.RollbackTransactionAsync();
    }

    public Task<int> SaveChangesAsync()
    {
      return _context.SaveChangesAsync();
    }
  }
}
