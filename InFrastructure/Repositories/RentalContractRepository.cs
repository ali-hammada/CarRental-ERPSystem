using ApplicationCore.Entities;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces.Repositories;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace InFrastructure.Repositories
{
    public class RentalContractRepository : GenericRepository<RentalContract>, IRentalContractRepository
    {
        public RentalContractRepository(AppDbContext context) : base(context) { }

        public IQueryable<RentalContract> GetOpenContracts()
        {
            return _dbSet.Where(r => r.Status == RentalContractStatus.Open);
        }

        public IQueryable<RentalContract> GetContractByCustomer(int customerId)
        {
            return _dbSet.Where(r => r.CustomerId == customerId);
        }

        public async Task<bool> HasActiveRentalAsync(int carId, DateTime? start, DateTime? end)
        {
            if (!start.HasValue || !end.HasValue) return false;

            return await _context.RentalContracts.AnyAsync(r =>
                r.CarId == carId &&
                (r.Status == RentalContractStatus.Open || r.Status == RentalContractStatus.Draft) &&
                start.Value.Date <= r.EndDate.Date &&
                end.Value.Date >= r.StartDate.Date);
        }
    }
}
