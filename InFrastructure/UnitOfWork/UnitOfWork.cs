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
      Customers=new GenericRepository<Customer>(_context);
      RentalContracts=new GenericRepository<RentalContract>(_context);
      Payments=new GenericRepository<Payment>(_context);

    }

    public ICarRepository Cars { get; }

    public IGenericRepository<Customer> Customers { get; }

    public IGenericRepository<RentalContract> RentalContracts { get; }

    public IGenericRepository<Payment> Payments { get; }

    public Task<int> SaveChangesAsync() => _context.SaveChangesAsync();
  }
}
