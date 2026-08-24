using ApplicationCore.Enums;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Providers
{
    public class DashboardCarPerformanceDto
    {
        public int CarId { get; set; }
        public string Model { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
        public string CategoryName { get; set; } = "Standard";
        public int TotalRentals { get; set; }
        public decimal TotalRevenue { get; set; }
        public string Status { get; set; } = null!;
        public string? ImageUrl { get; set; }
    }

    public class DashboardUpcomingReturnDto
    {
        public int RentalContractId { get; set; }
        public string CustomerName { get; set; } = null!;
        public string CarModel { get; set; } = null!;
        public string PlateNumber { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public decimal TotalAmount { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class DashboardPaymentMethodStatDto
    {
        public string MethodName { get; set; } = null!;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class DashboardMetricsDto
    {
        public int TotalRentals { get; set; }
        public int ActiveRentals { get; set; }
        public int CompletedRentals { get; set; }
        public int CancelledRentals { get; set; }
        public int OverdueRentalsCount { get; set; }

        public decimal TotalPaidAmount { get; set; }
        public decimal TotalRemainingUnpaid { get; set; }
        public decimal TotalMaintenanceCost { get; set; }
        
        // Sales Metrics
        public int TotalSalesCount { get; set; }
        public decimal TotalSalesRevenue { get; set; }
        public decimal CombinedGrossRevenue => TotalPaidAmount + TotalSalesRevenue;
        public decimal NetProfit => CombinedGrossRevenue - TotalMaintenanceCost;
        public int TotalPaymentsCount { get; set; }

        public int TotalCarsCount { get; set; }
        public int RentedCarsCount { get; set; }
        public int AvailableCarsCount { get; set; }
        public int MaintenanceCarsCount { get; set; }
        public int OutOfServiceCarsCount { get; set; }

        public double FleetUtilizationPercentage => TotalCarsCount > 0 ? Math.Round((double)RentedCarsCount / TotalCarsCount * 100, 1) : 0;

        public List<DashboardPaymentDto> RecentPayments { get; set; } = new List<DashboardPaymentDto>();
        public List<DashboardCarPerformanceDto> TopCars { get; set; } = new List<DashboardCarPerformanceDto>();
        public List<DashboardUpcomingReturnDto> UpcomingReturns { get; set; } = new List<DashboardUpcomingReturnDto>();
        public List<DashboardPaymentMethodStatDto> PaymentMethodStats { get; set; } = new List<DashboardPaymentMethodStatDto>();

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
            var salesQuery = _context.CarSaleContracts.AsNoTracking().Where(s => s.Status != SaleContractStatus.Cancelled);
            var currentDate = DateTime.UtcNow;

            if (employeeId.HasValue)
            {
                rentalsQuery = rentalsQuery.Where(r => r.EmployeeId == employeeId.Value);
                paymentsQuery = paymentsQuery.Where(p => p.RentalContract.EmployeeId == employeeId.Value);
                salesQuery = salesQuery.Where(s => s.EmployeeId == employeeId.Value);
            }

            var totalRentals = await rentalsQuery.CountAsync();
            var activeRentals = await rentalsQuery.CountAsync(r => r.Status == RentalContractStatus.Open);
            var completedRentals = await rentalsQuery.CountAsync(r => r.Status == RentalContractStatus.Closed);
            var cancelledRentals = await rentalsQuery.CountAsync(r => r.Status == RentalContractStatus.Cancelled);
            var overdueRentalsCount = await rentalsQuery.CountAsync(r => (r.Status == RentalContractStatus.Open && r.EndDate < currentDate) || r.Status == RentalContractStatus.Overdue);

            var totalSalesCount = await salesQuery.CountAsync();
            var totalSalesRevenue = await salesQuery.SumAsync(s => (decimal?)s.PaidAmount) ?? 0m;

            var totalPaidAmount = await paymentsQuery.SumAsync(p => (decimal?)p.Amount) ?? 0m;
            var totalPaymentsCount = await paymentsQuery.CountAsync();
            var totalRemainingUnpaid = await rentalsQuery
                .Where(r => r.Status == RentalContractStatus.Open || r.Status == RentalContractStatus.Overdue)
                .SumAsync(r => (decimal?)((r.FinalAmount ?? r.TotalAmount) - r.PaidAmount)) ?? 0m;

            var totalMaintenanceCost = await _context.MaintenanceLogs
                .AsNoTracking()
                .Where(m => !m.IsDeleted)
                .SumAsync(m => (decimal?)m.Cost) ?? 0m;

            var activeCarsQuery = _context.Cars.AsNoTracking().Where(c => c.SaleStatus == null || c.SaleStatus != CarSaleStatus.Sold);
            var totalCarsCount = await activeCarsQuery.CountAsync();
            var rentedCarsCount = await activeCarsQuery.CountAsync(c => c.Status == CarStatus.Rented);
            var availableCarsCount = await activeCarsQuery.CountAsync(c => c.Status == CarStatus.Available);
            var maintenanceCarsCount = await activeCarsQuery.CountAsync(c => c.Status == CarStatus.Maintenance);
            var outOfServiceCarsCount = await activeCarsQuery.CountAsync(c => c.Status == CarStatus.OutOfService);

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

            var topCars = await _context.Cars
                .AsNoTracking()
                .Select(c => new DashboardCarPerformanceDto
                {
                    CarId = c.Id,
                    Model = c.Model,
                    PlateNumber = c.PlateNumber,
                    CategoryName = c.Category != null ? c.Category.Name : "Standard",
                    TotalRentals = c.RentalContracts.Count(),
                    TotalRevenue = c.RentalContracts.Sum(r => (decimal?)r.PaidAmount) ?? 0m,
                    Status = c.Status.ToString(),
                    ImageUrl = c.ImageUrl
                })
                .OrderByDescending(c => c.TotalRevenue)
                .ThenByDescending(c => c.TotalRentals)
                .Take(5)
                .ToListAsync();

            var upcomingReturns = await rentalsQuery
                .Where(r => r.Status == RentalContractStatus.Open || r.Status == RentalContractStatus.Overdue)
                .OrderBy(r => r.EndDate)
                .Take(6)
                .Select(r => new DashboardUpcomingReturnDto
                {
                    RentalContractId = r.Id,
                    CustomerName = r.Customer.Name,
                    CarModel = r.Car.Model,
                    PlateNumber = r.Car.PlateNumber,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    TotalAmount = r.FinalAmount ?? r.TotalAmount,
                    IsOverdue = r.EndDate < currentDate || r.Status == RentalContractStatus.Overdue
                })
                .ToListAsync();

            var paymentMethodStats = await paymentsQuery
                .GroupBy(p => p.Method)
                .Select(g => new DashboardPaymentMethodStatDto
                {
                    MethodName = g.Key.ToString(),
                    Count = g.Count(),
                    TotalAmount = g.Sum(p => p.Amount)
                })
                .ToListAsync();

            var months = new List<string>();
            var rentalsPerMonth = new List<int>();
            var paymentsPerMonth = new List<decimal>();

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
                OverdueRentalsCount = overdueRentalsCount,
                TotalPaidAmount = totalPaidAmount,
                TotalSalesCount = totalSalesCount,
                TotalSalesRevenue = totalSalesRevenue,
                TotalRemainingUnpaid = Math.Max(0m, totalRemainingUnpaid),
                TotalMaintenanceCost = totalMaintenanceCost,
                TotalPaymentsCount = totalPaymentsCount,
                TotalCarsCount = totalCarsCount,
                RentedCarsCount = rentedCarsCount,
                AvailableCarsCount = availableCarsCount,
                MaintenanceCarsCount = maintenanceCarsCount,
                OutOfServiceCarsCount = outOfServiceCarsCount,
                RecentPayments = recentPayments,
                TopCars = topCars,
                UpcomingReturns = upcomingReturns,
                PaymentMethodStats = paymentMethodStats,
                Months = months,
                RentalsPerMonth = rentalsPerMonth,
                PaymentsPerMonth = paymentsPerMonth
            };
        }
    }
}
