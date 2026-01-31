using Application.Services.Interfaces;
using ApplicationCore.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using System.Security.Claims;

namespace Web.Controllers
{
  [Authorize]
  public class PaymentController:Controller
  {
    private readonly IPaymentServices _paymentService;
    private readonly IRentalServices _rentalService;
    private readonly IToastNotification _toast;

    public PaymentController(IPaymentServices paymentServices,IRentalServices rentalServices,IToastNotification toast)
    {
      _paymentService=paymentServices;
      _rentalService=rentalServices;
      _toast=toast;
    }

    [HttpGet]
    public async Task<IActionResult> Index()
    {
      int employeeId = GetCurrentEmployeeId();
      var rentals = await _rentalService.GetEmployeesRentalsAsync(employeeId);
      var unpaidRentals = rentals.Where(r =>
          r.Status==RentalContractStatus.Open||
          (r.FinalAmount??r.TotalAmount)>r.PaidAmount
      ).ToList();
      return View(unpaidRentals);
    }

    [HttpGet]
    public async Task<IActionResult> MakePayment(int rentalId)
    {
      int employeeId = GetCurrentEmployeeId();
      var (success, message, remaining)=await _paymentService.GetRemainingAmountAsync(rentalId,employeeId);
      if(!success)
      {
        _toast.AddErrorToastMessage(message);
        return RedirectToAction(nameof(Index));
      }
      ViewBag.RentalId=rentalId;
      ViewBag.RemainingAmount=remaining;
      return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakePayment(int rentalId,decimal amount,PaymentPurpose purpose,PaymentMethod method)
    {
      int employeeId = GetCurrentEmployeeId();
      var result = await _paymentService.MakePaymentAsync(rentalId,amount,purpose,method,employeeId);
      if(!result.success)
      {
        _toast.AddErrorToastMessage(result.message);
        ViewBag.RentalId=rentalId;
        var (_, __, remaining)=await _paymentService.GetRemainingAmountAsync(rentalId,employeeId);
        ViewBag.RemainingAmount=remaining;
        return View();
      }
      _toast.AddSuccessToastMessage(result.message);
      return RedirectToAction("Details","Rentals",new { rentalId = rentalId });
    }

    [HttpGet]
    public async Task<IActionResult> History(int? rentalId = null)
    {
      int employeeId = GetCurrentEmployeeId();
      if(rentalId.HasValue)
      {
        var payments = await _paymentService.GetContractPaymentsAsync(rentalId.Value,employeeId);
        ViewBag.RentalId=rentalId.Value;
        return View(payments);
      }
      else
      {
        var allPayments = await _paymentService.GetAllEmployeesPaymentsAsync(employeeId);
        return View("AllPayments",allPayments);
      }
    }

    [HttpGet]
    public async Task<IActionResult> Receipt(int paymentId)
    {
      int employeeId = GetCurrentEmployeeId();
      var payment = await _paymentService.GetPaymentByIdAsync(paymentId,employeeId);
      if(payment==null)
      {
        _toast.AddErrorToastMessage("Payment not found");
        return RedirectToAction(nameof(Index));
      }
      return View(payment);
    }

    private int GetCurrentEmployeeId()
    {
      var employeeIdClaim = User.FindFirst("CustomerId")?.Value
                         ??User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
      if(string.IsNullOrEmpty(employeeIdClaim))
        throw new UnauthorizedAccessException("User is not authenticated");
      return int.Parse(employeeIdClaim);
    }
  }
}