using Application.Services.Interfaces;
using ApplicationCore.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels;

namespace Web.Controllers
{
  [Authorize]
  public class HomeController:Controller
  {
    private readonly IRentalServices _rentalServices;
    private readonly IPaymentServices _paymentServices;
    private readonly ICarServices _carServices;

    public HomeController(IRentalServices rentalServices,IPaymentServices paymentServices,ICarServices carServices)
    {
      _rentalServices=rentalServices;
      _paymentServices=paymentServices;
      _carServices=carServices;
    }

    private int GetCurrentCustomerId()
    {
      var customerIdClaim = User.FindFirst("CustomerId")?.Value
                         ??User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

      if(string.IsNullOrEmpty(customerIdClaim))
        throw new UnauthorizedAccessException("User not authenticated");

      return int.Parse(customerIdClaim);
    }

    public async Task<IActionResult> Index()
    {
      return RedirectToAction("Dashboard");
    }

    public async Task<IActionResult> Dashboard()
    {
      int customerId = GetCurrentCustomerId();

      // جلب جميع البيانات المطلوبة
      var rentals = await _rentalServices.GetCustomerRentalsAsync(customerId);
      var payments = await _paymentServices.GetAllCustomerPaymentsAsync(customerId);
      foreach(var rental in rentals)
      {
        var car = await _carServices.GetByIdAsync(rental.CarId);
        rental.Car=car;
      }
      // حساب الإحصائيات
      var activeRentals = rentals.Where(r => r.Status==RentalContractStatus.Open).ToList();
      var completedRentals = rentals.Where(r =>
          r.Status==RentalContractStatus.Closed||
          r.Status==RentalContractStatus.Cancelled).ToList();

      var viewModel = new DashboardViewModel
      {
        TotalRentals=rentals.Count,
        ActiveRentals=activeRentals.Count,
        CompletedRentals=completedRentals.Count,
        TotalPaidAmount=payments.Sum(p => p.Amount),
        TotalPayments=payments.Count,
        CarsRented=rentals.Select(r => r.CarId).Distinct().Count(),
        ActiveContracts=activeRentals.OrderByDescending(r => r.StartDate).Take(5).ToList(),
        RecentPayments=payments.OrderByDescending(p => p.PaymentDate).Take(5).ToList()
      };


      var months = new List<string>();
      var rentalsPerMonth = new List<int>();
      var paymentsPerMonth = new List<decimal>();

      var currentDate = DateTime.Now;
      for(int i = 5;i>=0;i--)
      {
        var monthDate = currentDate.AddMonths(-i);
        var monthName = monthDate.ToString("MMM yyyy");
        months.Add(monthName);

        // عدد العقود في هذا الشهر
        var rentalCount = rentals.Count(r =>
            r.StartDate.Year==monthDate.Year&&
            r.StartDate.Month==monthDate.Month);
        rentalsPerMonth.Add(rentalCount);

        // مجموع المدفوعات في هذا الشهر
        var paymentSum = payments.Where(p =>
            p.PaymentDate.Year==monthDate.Year&&
            p.PaymentDate.Month==monthDate.Month).Sum(p => p.Amount);
        paymentsPerMonth.Add(paymentSum);
      }

      viewModel.Months=months;
      viewModel.RentalsPerMonth=rentalsPerMonth;
      viewModel.PaymentsPerMonth=paymentsPerMonth;

      return View(viewModel);
    }
  }
}