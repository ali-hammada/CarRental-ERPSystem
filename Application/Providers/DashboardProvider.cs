using ApplicationCore.Enums;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Providers
{
    public class DashboardMetricsDto
    {
        public int TotalRentals { get; set; }
        public int ActiveRentals { get; set; }
        public int CompletedRentals { get; set; }
        public int CancelledRentals { get; set; }
        public decimal TotalPaidAmount { get; set; }
        public int TotalPaymentsCount { get; set; }
        public int TotalCarsCount { get; set; }
        public int RentedCarsCount { get; set; }
        public int AvailableCarsCount { get; set; }
        public double FleetUtilizationPercentage => TotalCarsCount > 0 ? Math.Round((double)RentedCarsCount / TotalCarsCount * 100, 1) : 0;
        public List<DashboardPaymentDto> RecentPayments { get; set; } = new List<DashboardPaymentDto>();
        public List<string> Months { get; set; } = new List<string>();
        public List<int> RentalsPerMonth { get; set; } = new List<int>();
        public List<decimal> PaymentsPerMonth { get; set; } = new List<decimal>();
    }

    public class DashboardPaymentDto
    {
        public int Id { get; set; }
        public int RentalContractId { get; set; }
        public string CustomerName { get; set; } = null!;
        public decimal Amount { get; set; }
        public PaymentPurpose Purpose { get; set; }
        public PaymentMethod Method { get; set; }
        public DateTime PaymentDate { get; set; }
    }

    public interface IDashboardProvider
    {
        Task<DashboardMetricsDto> GetDashboardMetricsAsync(int? employeeId = null);
    }

    public class DashboardProvider : IDashboardProvider
    {
        private readonly AppDbContext _context;

        public DashboardProvider(AppDbContext context)
        {
            _context = context;
        }

        public async Task<DashboardMetricsDto> GetDashboardMetricsAsync(int? employeeId = null)
        {
            var rentalsQuery = _context.RentalContracts.AsNoTracking();
            var paymentsQuery = _context.Payments.AsNoTracking();

            if (employeeId.HasValue)
            {
                rentalsQuery = rentalsQuery.Where(r => r.EmployeeId == employeeId.Value);
                paymentsQuery = paymentsQuery.Where(p => p.RentalContract.EmployeeId == employeeId.Value);
            }

            var totalRentals = await rentalsQuery.CountAsync();
            var activeRentals = await rentalsQuery.CountAsync(r => r.Status == RentalContractStatus.Open);
            var completedRentals = await rentalsQuery.CountAsync(r => r.Status == RentalContractStatus.Closed);
            var cancelledRentals = await rentalsQuery.CountAsync(r => r.Status == RentalContractStatus.Cancelled);

            var totalPaidAmount = await paymentsQuery.SumAsync(p => (decimal?)p.Amount) ?? 0m;
            var totalPaymentsCount = await paymentsQuery.CountAsync();

            var totalCarsCount = await _context.Cars.AsNoTracking().CountAsync();
            var rentedCarsCount = await _context.Cars.AsNoTracking().CountAsync(c => c.Status == CarStatus.Rented);
            var availableCarsCount = await _context.Cars.AsNoTracking().CountAsync(c => c.Status == CarStatus.Available);

            var recentPayments = await paymentsQuery
                .OrderByDescending(p => p.PaymentDate)
                .Take(5)
                .Select(p => new DashboardPaymentDto
                {
                    Id = p.Id,
                    RentalContractId = p.RentalContractId,
                    CustomerName = p.RentalContract.Customer.Name,
                    Amount = p.Amount,
                    Purpose = p.Purpose,
                    Method = p.Method,
                    PaymentDate = p.PaymentDate
                })
                .ToListAsync();

            var months = new List<string>();
            var rentalsPerMonth = new List<int>();
            var paymentsPerMonth = new List<decimal>();

            var currentDate = DateTime.UtcNow;
            for (int i = 6; i >= 0; i--)
            {
                var monthDate = currentDate.AddMonths(-i);
                var monthName = monthDate.ToString("MMM yyyy");
                months.Add(monthName);

                var rentalCount = await rentalsQuery.CountAsync(r =>
                    r.StartDate.Year == monthDate.Year &&
                    r.StartDate.Month == monthDate.Month);
                rentalsPerMonth.Add(rentalCount);

                var paymentSum = await paymentsQuery.Where(p =>
                    p.PaymentDate.Year == monthDate.Year &&
                    p.PaymentDate.Month == monthDate.Month).SumAsync(p => (decimal?)p.Amount) ?? 0m;
                paymentsPerMonth.Add(paymentSum);
            }

            return new DashboardMetricsDto
            {
                TotalRentals = totalRentals,
                ActiveRentals = activeRentals,
                CompletedRentals = completedRentals,
                CancelledRentals = cancelledRentals,
                TotalPaidAmount = totalPaidAmount,
                TotalPaymentsCount = totalPaymentsCount,
                TotalCarsCount = totalCarsCount,
                RentedCarsCount = rentedCarsCount,
                AvailableCarsCount = availableCarsCount,
                RecentPayments = recentPayments,
                Months = months,
                RentalsPerMonth = rentalsPerMonth,
                PaymentsPerMonth = paymentsPerMonth
            };
        }
    }
}
