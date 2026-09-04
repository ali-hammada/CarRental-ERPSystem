using Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
    [Authorize]
    public class InvoicesController : Controller
    {
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(IInvoiceService invoiceService)
        {
            _invoiceService = invoiceService;
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
                return RedirectToAction(nameof(Index));
            }

            return View(invoice);
        }
    }
}
