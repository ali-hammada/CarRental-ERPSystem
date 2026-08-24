using ApplicationCore.Entities;
using ApplicationCore.Interfaces.Repositories;

namespace ApplicationCore.Interfaces
{
    public interface IUnitOfWork
    {
        ICarRepository Cars { get; }
        ICustomerRepository Customer { get; }
        IEmployeeRepository Employee { get; }
        IRentalContractRepository RentalContracts { get; }
        IGenericRepository<Payment> Payments { get; }
        IGenericRepository<CarCategory> CarCategories { get; }
        IGenericRepository<MaintenanceLog> MaintenanceLogs { get; }
        IGenericRepository<Invoice> Invoices { get; }
        IGenericRepository<CarSaleContract> CarSaleContracts { get; }
        IGenericRepository<SaleInstallment> SaleInstallments { get; }

        Task BeginTransactionAsync();
        Task CommitAsync();
        Task RollbackAsync();
        Task<int> SaveChangesAsync();
    }
}
