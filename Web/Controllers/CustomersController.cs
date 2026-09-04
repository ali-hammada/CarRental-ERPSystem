using Application.Providers;
using Application.Services;
using ApplicationCore.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomerProvider _customerProvider;
        private readonly ICustomerServices _customerServices;

        public CustomersController(
            ICustomerProvider customerProvider,
            ICustomerServices customerServices)
        {
            _customerProvider = customerProvider;
            _customerServices = customerServices;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var customers = await _customerProvider.GetAllCustomersAsync();
                return View(customers);
            }
            catch
            {
                return View(new List<Application.DTOs.CustomerListDto>());
            }
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("CreateEdit", new Customer { LicenseExpiryDate = DateTime.Today.AddYears(1) });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Customer customer)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            ModelState.Remove("PasswordHash");
            ModelState.Remove("RentalContracts");

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join("<br>", errors) });
                }
                return View("CreateEdit", customer);
            }

            try
            {
                customer.PasswordHash ??= string.Empty;
                await _customerServices.AddCustomerAsync(customer);
                if (isAjax) return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (isAjax) return Json(new { success = false, message = $"Failed to add customer: {ex.Message}" });
                return View("CreateEdit", customer);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var customer = await _customerServices.GetByIdAsync(id);
                if (customer == null)
                {
                    return RedirectToAction(nameof(Index));
                }

                return View("CreateEdit", customer);
            }
            catch
            {
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            ModelState.Remove("PasswordHash");
            ModelState.Remove("RentalContracts");

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join("<br>", errors) });
                }
                return View("CreateEdit", customer);
            }

            try
            {
                await _customerServices.UpdateCustomerAsync(customer);
                if (isAjax) return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                if (isAjax) return Json(new { success = false, message = $"Failed to update customer: {ex.Message}" });
                return View("CreateEdit", customer);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            try
            {
                var result = await _customerServices.DeleteCustomerAsync(id);
                if (isAjax) return Json(new { success = result, redirectUrl = Url.Action(nameof(Index)) });
            }
            catch (Exception ex)
            {
                if (isAjax) return Json(new { success = false, message = ex.Message });
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
