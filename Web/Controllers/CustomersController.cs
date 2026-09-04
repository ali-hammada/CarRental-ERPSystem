using Application.Providers;
using Application.Services;
using ApplicationCore.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace Web.Controllers
{
    [Authorize]
    public class CustomersController : Controller
    {
        private readonly ICustomerProvider _customerProvider;
        private readonly ICustomerServices _customerServices;
        private readonly IToastNotification _toast;

        public CustomersController(
            ICustomerProvider customerProvider,
            ICustomerServices customerServices,
            IToastNotification toast)
        {
            _customerProvider = customerProvider;
            _customerServices = customerServices;
            _toast = toast;
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
                _toast.AddErrorToastMessage("Error loading customer catalog.");
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
            ModelState.Remove("PasswordHash");
            ModelState.Remove("RentalContracts");

            if (!ModelState.IsValid)
            {
                _toast.AddInfoToastMessage("Please fix validation errors.");
                return View("CreateEdit", customer);
            }

            try
            {
                customer.PasswordHash ??= string.Empty;
                await _customerServices.AddCustomerAsync(customer);
                _toast.AddSuccessToastMessage("Customer added successfully.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _toast.AddErrorToastMessage($"Failed to add customer: {ex.Message}");
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
                    _toast.AddWarningToastMessage("Customer record not found.");
                    return RedirectToAction(nameof(Index));
                }

                return View("CreateEdit", customer);
            }
            catch
            {
                _toast.AddErrorToastMessage("Error loading customer details.");
                return RedirectToAction(nameof(Index));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Customer customer)
        {
            ModelState.Remove("PasswordHash");
            ModelState.Remove("RentalContracts");

            if (!ModelState.IsValid)
            {
                _toast.AddInfoToastMessage("Please fix validation errors.");
                return View("CreateEdit", customer);
            }

            try
            {
                await _customerServices.UpdateCustomerAsync(customer);
                _toast.AddSuccessToastMessage("Customer profile updated.");
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _toast.AddErrorToastMessage($"Failed to update customer: {ex.Message}");
                return View("CreateEdit", customer);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var result = await _customerServices.DeleteCustomerAsync(id);

                if (!result)
                    _toast.AddWarningToastMessage("Customer could not be deleted.");
                else
                    _toast.AddSuccessToastMessage("Customer profile removed.");
            }
            catch
            {
                _toast.AddErrorToastMessage("Failed to delete customer.");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
