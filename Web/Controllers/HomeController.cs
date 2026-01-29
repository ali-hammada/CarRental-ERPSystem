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

    private int GetCurrentEmployeeId()
    {
      var employeeIdClaim = User.FindFirst("EmployeeId")?.Value
                         ??User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

      if(string.IsNullOrEmpty(employeeIdClaim))
        throw new UnauthorizedAccessException("User not authenticated");

      return int.Parse(employeeIdClaim);
    }

    public IActionResult Index()
    {
      return RedirectToAction(nameof(Dashboard));
    }

    public async Task<IActionResult> Dashboard()
    {
      int employeeId = GetCurrentEmployeeId();

      var rentals = await _rentalServices.GetEmployeesRentalsWithCarsAsync(employeeId);
      var payments = await _paymentServices.GetAllEmployeesPaymentsAsync(employeeId);

      var activeRentals = rentals.Where(r => r.Status==RentalContractStatus.Open).ToList();
      var completedRentals = rentals.Where(r =>
          r.Status==RentalContractStatus.Closed).ToList();
      var cancelledRentalls = rentals.Where(r =>
          r.Status==RentalContractStatus.Cancelled).ToList();


      var viewModel = new DashboardViewModel
      {
        TotalRentals=rentals.Count,
        ActiveRentals=activeRentals.Count,
        CompletedRentals=completedRentals.Count,
        CancelledRentals=cancelledRentalls.Count,
        TotalPaidAmount=payments.Sum(p => p.Amount),
        TotalPayments=payments.Count,
        CarsRented=activeRentals.Select(r => r.CarId).Distinct().Count(),
        CarsAvailable=activeRentals.Select(r => r.Car.Status!=CarStatus.Rented).Distinct().Count(),
        ActiveContracts=activeRentals.OrderByDescending(r => r.StartDate).Take(5).ToList(),
        RecentPayments=payments.OrderByDescending(p => p.PaymentDate).Take(5).ToList()
      };


      var months = new List<string>();
      var rentalsPerMonth = new List<int>();
      var paymentsPerMonth = new List<decimal>();

      var currentDate = DateTime.Now;
      for(int i = 6;i>=0;i--)
      {
        var monthDate = currentDate.AddMonths(-i);
        var monthName = monthDate.ToString("MMM yyyy");
        months.Add(monthName);

        // عدد العقود في الشهر
        var rentalCount = rentals.Count(r =>
            r.StartDate.Year==monthDate.Year&&
            r.StartDate.Month==monthDate.Month);
        rentalsPerMonth.Add(rentalCount);

        // مجموع المدفوعات في الشهر
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