using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Repositories;
using InFrastructure.Data;
using InFrastructure.Repositories;

namespace InFrastructure.UnitOfWork
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly AppDbContext _context;

        public UnitOfWork(AppDbContext context)
        {
            _context = context;

            Cars = new CarRepository(_context);
            Customer = new CustomerRepository(_context);
            Employee = new EmployeeRepository(_context);
            RentalContracts = new RentalContractRepository(_context);
            Payments = new GenericRepository<Payment>(_context);
            CarCategories = new GenericRepository<CarCategory>(_context);
            MaintenanceLogs = new GenericRepository<MaintenanceLog>(_context);
            Invoices = new GenericRepository<Invoice>(_context);
        }

        public ICarRepository Cars { get; }
        public ICustomerRepository Customer { get; }
        public IEmployeeRepository Employee { get; }
        public IRentalContractRepository RentalContracts { get; }
        public IGenericRepository<Payment> Payments { get; }
        public IGenericRepository<CarCategory> CarCategories { get; }
        public IGenericRepository<MaintenanceLog> MaintenanceLogs { get; }
        public IGenericRepository<Invoice> Invoices { get; }

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
