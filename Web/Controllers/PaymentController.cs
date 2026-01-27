using Application.Services.Interfaces;
using ApplicationCore.Entities.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Web.Controllers
{
  [Authorize]
  public class PaymentController:Controller
  {
    private readonly IPaymentServices _paymentService;
    private readonly IRentalServices _rentalService;

    public PaymentController(IPaymentServices paymentServices,IRentalServices rentalServices)
    {
      _paymentService=paymentServices;
      _rentalService=rentalServices;
    }
    [HttpGet]
    public async Task<IActionResult> Index()
    {
      int customerId = GetCurrentCustomerId();


      var rentals = await _rentalService.GetCustomerRentalsAsync(customerId);

      var unpaidRentals = rentals.Where(r =>
          r.Status==RentalContractStatus.Open||
          (r.FinalAmount??r.TotalAmount)>r.PaidAmount
      ).ToList();

      return View(unpaidRentals);
    }

    [HttpGet]
    public async Task<IActionResult> MakePayment(int rentalId)
    {
      int customerId = GetCurrentCustomerId();

      var (success, message, remaining)=await _paymentService.GetRemainingAmountAsync(rentalId,customerId);

      if(!success)
      {
        TempData["Error"]=message;
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
      int customerId = GetCurrentCustomerId();

      var result = await _paymentService.MakePaymentAsync(rentalId,amount,purpose,method,customerId);

      if(!result.success)
      {
        TempData["Error"]=result.message;
        ViewBag.RentalId=rentalId;

        var (_, __, remaining)=await _paymentService.GetRemainingAmountAsync(rentalId,customerId);
        ViewBag.RemainingAmount=remaining;

        return View();
      }

      TempData["Success"]=result.message;
      return RedirectToAction("Details","Rental",new { id = rentalId });
    }

    [HttpGet]
    public async Task<IActionResult> History(int? rentalId = null)
    {
      int customerId = GetCurrentCustomerId();

      if(rentalId.HasValue)
      {
        var payments = await _paymentService.GetContractPaymentsAsync(rentalId.Value,customerId);
        ViewBag.RentalId=rentalId.Value;
        return View(payments);
      }
      else
      {
        var allPayments = await _paymentService.GetAllCustomerPaymentsAsync(customerId);
        return View("AllPayments",allPayments);
      }
    }
    [HttpGet]
    public async Task<IActionResult> Receipt(int paymentId)
    {
      int customerId = GetCurrentCustomerId();
      var payment = await _paymentService.GetPaymentByIdAsync(paymentId,customerId);

      if(payment==null)
      {
        TempData["Error"]="Payment not found";
        return RedirectToAction("Index");
      }

      return View(payment);
    }


    private int GetCurrentCustomerId()
    {
      var customerIdClaim = User.FindFirst("CustomerId")?.Value
                         ??User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

      if(string.IsNullOrEmpty(customerIdClaim))
        throw new UnauthorizedAccessException("User is not authenticated");

      return int.Parse(customerIdClaim);
    }
  }
}