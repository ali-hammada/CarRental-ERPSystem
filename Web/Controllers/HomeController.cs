using Application.Services.Interfaces;
using ApplicationCore.Entities.Enums;
using InFrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Localization;
using System.Security.Claims;
using Web.Resources;
using Web.ViewModels;

namespace Web.Controllers
{
  [Authorize(Policy = "AdminOnly")]
  public class HomeController:Controller
  {
    private readonly IRentalServices _rentalServices;
    private readonly IPaymentServices _paymentServices;
    private readonly AppDbContext _dbContext;
    private readonly IStringLocalizer<SharedResources> _localizer;


    public HomeController(IRentalServices rentalServices,IPaymentServices paymentServices,AppDbContext dbContext,IStringLocalizer<SharedResources> localizer)
    {
      _rentalServices=rentalServices;
      _paymentServices=paymentServices;
      _dbContext=dbContext;
      _localizer=localizer;
    }

    private int GetCurrentEmployeeId()
    {
      var employeeIdClaim = User.FindFirst("EmployeeId")?.Value
                         ??User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      if(string.IsNullOrEmpty(employeeIdClaim))
        throw new UnauthorizedAccessException("User is not authenticated");
      return int.Parse(employeeIdClaim);
    }

    public IActionResult Index()
    {
      ViewData["Welcome"]=_localizer["Welcome"];
      return RedirectToAction(nameof(Dashboard));
    }
    [HttpGet]
    public async Task<IActionResult> GetPartialRecentPayments(int page = 1,int pageSize = 5)
    {
      int employeeId = GetCurrentEmployeeId();
      var payments = await _paymentServices.GetAllEmployeesPaymentsAsync(employeeId);

      var totalPayments = payments.Count();
      var recentPayments = _dbContext.Payments.OrderByDescending(p => p.PaymentDate).Skip((page-1)*pageSize).Take(pageSize).ToList();
      var totlaPages = (int)Math.Ceiling((double)totalPayments/pageSize);


      var Vm = new DashboardViewModel
      {
        RecentPayments=recentPayments,
        CurrentPage=page,
        TotalPages=totlaPages,
        PageSize=pageSize

      };
      return PartialView("_RecentPaymentsPartial",Vm);

    }

    public async Task<IActionResult> Dashboard()
    {
      int employeeId = GetCurrentEmployeeId();

      int page = 1;
      int pageSize = 5;
      var rentals = await _rentalServices.GetEmployeesRentalsWithCarsAsync(employeeId);
      var payments = await _paymentServices.GetAllEmployeesPaymentsAsync(employeeId);

      var activeRentals = rentals.Where(r => r.Status==RentalContractStatus.Open).ToList();
      var completedRentals = rentals.Where(r => r.Status==RentalContractStatus.Closed).ToList();
      var cancelledRentalls = rentals.Where(r => r.Status==RentalContractStatus.Cancelled).ToList();

      var totalPayments = payments.Count();
      var recentPayments = _dbContext.Payments.OrderByDescending(p => p.PaymentDate).Skip((page-1)*pageSize).Take(pageSize).ToList();
      var totlaPages = (int)Math.Ceiling((double)totalPayments/pageSize);




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
        RecentPayments=recentPayments,
        CurrentPage=page,
        TotalPages=totlaPages,
        PageSize=pageSize

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