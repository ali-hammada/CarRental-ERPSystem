using Application.Services.DTO_s;
using Application.Services.Interfaces;
using ApplicationCore.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels;

namespace Web.Controllers
{
  [Authorize]
  public class RentalsController:Controller
  {
    private readonly IRentalServices _rentalServices;
    private readonly IPaymentServices _paymentServices;
    private readonly ICarServices _carServices;


    public RentalsController(IRentalServices rentalServices,IPaymentServices paymentServices,ICarServices carServices)
    {
      _rentalServices=rentalServices;
      _paymentServices=paymentServices;
      _carServices=carServices;

    }

    // معرفه العميل الحالي 
    private int GetCurrentEmployeeId()
    {
      var employeeIdClaim = User.FindFirst("EmployeeId")?.Value
                         ??User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

      if(string.IsNullOrEmpty(employeeIdClaim))
        throw new UnauthorizedAccessException("User not authenticated");

      return int.Parse(employeeIdClaim);
    }
    public async Task<IActionResult> Index()
    {
      int customerId = GetCurrentEmployeeId();
      var rentals = await _rentalServices.GetEmployeesRentalsAsync(customerId);
      return View(rentals);
    }
    [HttpGet]

    [HttpGet]
    public async Task<IActionResult> Open(int carId)
    {
      var car = await _carServices.GetByIdAsync(carId);

      if(car==null||car.Status!=CarStatus.Available)
      {
        TempData["Error"]="Car not available";
        return RedirectToAction("Index","Car");
      }

      ViewBag.Car=car;

      return View(new RentalRequestDTO
      {
        CarId=carId
      });
    }
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Open(RentalRequestDTO request)
    {
      if(!ModelState.IsValid)
        return View(request);

      int customerId = GetCurrentEmployeeId();
      var result = await _rentalServices.OpenRequestRentalAsync(request,customerId);

      if(!result.Success)
      {
        ModelState.AddModelError("",result.Content);
        return View(request);
      }

      TempData["Success"]="Contract opened successfully!";
      return RedirectToAction("Index");
    }

    // إلغاء عقد
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int rentalId,string? reason)
    {
      int customerId = GetCurrentEmployeeId();
      var result = await _rentalServices.CancelRentalAsync(rentalId,customerId,reason);

      if(!result.Success)
      {
        TempData["Error"]=result.Content;
      }
      else
      {
        TempData["Success"]=result.Content;
      }

      return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Pay(int rentalId)
    {
      int employeeId = GetCurrentEmployeeId();
      var rental = await _rentalServices.GetRentalByIdAsync(rentalId);

      if(rental==null||rental.EmployeeId!=employeeId)
      {
        TempData["Error"]="Rental not found";
        return RedirectToAction("Index");
      }

      var remaining = await _paymentServices.GetRemainingAmountAsync(rentalId,employeeId);
      ViewBag.RentalId=rentalId;
      ViewBag.RemainingAmount=remaining.remaining;

      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Pay(int rentalId,decimal amount,PaymentPurpose purpose,PaymentMethod method)
    {
      int employeeId = GetCurrentEmployeeId();

      var result = await _paymentServices.MakePaymentAsync(rentalId,amount,purpose,method,employeeId);

      if(!result.success)
      {
        TempData["Error"]=result.message;
        var remaining = await _paymentServices.GetRemainingAmountAsync(rentalId,employeeId);
        ViewBag.RentalId=rentalId;
        ViewBag.RemainingAmount=remaining.remaining;
        return View();
      }

      TempData["Success"]=result.message;
      return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Details(int rentalId)
    {
      int employeeId = GetCurrentEmployeeId();
      var rental = await _rentalServices.GetRentalByIdAsync(rentalId);

      if(rental==null||rental.EmployeeId!=employeeId)
      {
        TempData["Error"]="Rental not found";
        return RedirectToAction("Index");
      }

      var payments = await _paymentServices.GetContractPaymentsAsync(rentalId,employeeId);
      ViewBag.Payments=payments;
      return View(rental);
    }



    [HttpGet]
    public async Task<IActionResult> Extend(int rentalId)
    {
      int employeeId = GetCurrentEmployeeId();

      var rental = await _rentalServices.GetRentalByIdAsync(rentalId);

      if(rental==null||rental.EmployeeId!=employeeId)
      {
        TempData["Error"]="Rental not found";
        return RedirectToAction("Index");
      }
      var car = await _carServices.GetByIdAsync(rental.CarId);
      ViewBag.Car=car;

      var model = new ExtendRentalDto
      {
        RentalId=rental.Id,
        NewEndDate=rental.EndDate.AddDays(1),
        Notes=rental.Notes
      };

      return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Close(int rentalId)
    {
      int employeeId = GetCurrentEmployeeId();
      var rental = await _rentalServices.GetRentalByIdAsync(rentalId);

      if(rental==null||rental.EmployeeId!=employeeId)
      {
        TempData["Error"]="Rental not found";
        return RedirectToAction("Index");
      }

      if(rental.Status!=ApplicationCore.Entities.Enums.RentalContractStatus.Open)
      {
        TempData["Error"]="Only active rentals can be closed";
        return RedirectToAction("Details",new { rentalId = rentalId });
      }

      // تحميل بيانات السيارة
      var car = await _carServices.GetByIdAsync(rental.CarId);
      ViewBag.Rental=rental;
      ViewBag.Car=car;

      var model = new RentalCloseDto
      {
        RentalId=rental.Id
      };

      return View(model);
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Close(RentalCloseDto request)
    {
      int employeeId = GetCurrentEmployeeId();

      var result = await _rentalServices.CloseContractAsync(request,employeeId);

      if(!result.Success)
      {
        TempData["Error"]=result.Content;
        var rental = await _rentalServices.GetRentalByIdAsync(request.RentalId);
        ViewBag.Rental=rental;
        ViewBag.Car=await _carServices.GetByIdAsync(rental?.CarId??0);

        return View(request);
      }

      TempData["Success"]=result.Content;
      return RedirectToAction("Index");
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Extend(ExtendRentalDto extend)
    {
      if(!ModelState.IsValid)
        return View(extend);

      int employeeId = GetCurrentEmployeeId();

      var result = await _rentalServices.ExtendContractAsync(extend,employeeId);

      if(!result.Success)
      {
        ModelState.AddModelError("",result.Content);
        ViewBag.Car=await _carServices.GetByIdAsync(extend.RentalId);
        return View(extend);
      }

      TempData["Success"]="Rental extended successfully!";
      return RedirectToAction("Details",new { rentalId = result.id });
    }

    [HttpGet]
    public async Task<IActionResult> Active(string? searchTerm)
    {
      int employeeId = GetCurrentEmployeeId();
      var rentals = await _rentalServices.GetEmployeesRentalsAsync(employeeId);

      var activeRentals = rentals
          .Where(r => r.Status==ApplicationCore.Entities.Enums.RentalContractStatus.Open)
          .ToList();
      foreach(var rental in activeRentals)
      {
        var car = await _carServices.GetByIdAsync(rental.CarId);
        rental.Car=car;
      }

      var model = new ActiveRentalsViewModel
      {
        Rentals=activeRentals,
        SearchTerm=searchTerm
      };

      return View(model);
    }

  }
}
