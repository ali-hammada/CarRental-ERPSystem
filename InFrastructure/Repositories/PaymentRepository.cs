using ApplicationCore.Entities;
using InFrastructure.Data;

namespace InFrastructure.Repositories
{
  public class PaymentRepository:GenericRepository<Payment>
  {
    public PaymentRepository(AppDbContext context) : base(context)
    {
    }
    public IQueryable<Payment> GetPaymentsByContract(int rentalContID)
    {
      return _dbSet.Where(p => p.RentalContractId==rentalContID);

    }

    public IQueryable<Payment> GetPaymentsInPeriod(DateTime start,DateTime end)
    {

      return _dbSet.Where(p => p.PaymentDate>=start&&p.PaymentDate<=end);

    }
  }
}
