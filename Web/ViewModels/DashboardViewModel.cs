using ApplicationCore.Entities;

namespace Web.ViewModels
{
  public class DashboardViewModel
  {
    // إحصائيات رئيسية
    public int TotalRentals { get; set; }
    public int ActiveRentals { get; set; }
    public int CompletedRentals { get; set; }
    public decimal TotalPaidAmount { get; set; }
    public int TotalPayments { get; set; }
    public int CarsRented { get; set; }

    // العقود النشطة
    public List<RentalContract> ActiveContracts { get; set; } = new();

    // المدفوعات الأخيرة
    public List<Payment> RecentPayments { get; set; } = new();

    // بيانات الرسوم البيانية (آخر 6 أشهر)
    public List<string> Months { get; set; } = new();
    public List<int> RentalsPerMonth { get; set; } = new();
    public List<decimal> PaymentsPerMonth { get; set; } = new();
  }
}
