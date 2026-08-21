using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace Web.Controllers
{
    [Authorize]
    public class InvoicesController : Controller
    {
        private readonly IInvoiceService _invoiceService;
        private readonly IToastNotification _toast;

        public InvoicesController(IInvoiceService invoiceService, IToastNotification toast)
        {
            _invoiceService = invoiceService;
            _toast = toast;
        }

        public async Task<IActionResult> Index()
        {
            var invoices = await _invoiceService.GetAllInvoicesAsync();
            return View(invoices);
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            var invoice = await _invoiceService.GetByIdAsync(id);
            if (invoice == null)
            {
                _toast.AddErrorToastMessage("Invoice record not found.");
                return RedirectToAction(nameof(Index));
            }

            return View(invoice);
        }
    }
}
