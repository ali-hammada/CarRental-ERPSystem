using Application.Services.Interfaces;
using ApplicationCore.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;

namespace Web.Controllers
{
  [Authorize]
  public class CustomersController:Controller
  {
    private readonly ICustomerServices _customerServices;
    private readonly IToastNotification _toast;

    public CustomersController(ICustomerServices customerServices,IToastNotification toast)
    {
      _customerServices=customerServices;
      _toast=toast;
    }

    public async Task<IActionResult> Index()
    {
      try
      {
        var customers = await _customerServices.GetAllCustomersAsync();
        return View(customers);
      }
      catch
      {
        _toast.AddErrorToastMessage("حدث خطأ أثناء تحميل قائمة العملاء");
        return View(new List<Customer>());
      }
    }

    [HttpGet]
    public IActionResult Create()
    {
      return View(new Customer());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Customer customer)
    {
      if(!ModelState.IsValid)
      {
        _toast.AddInfoToastMessage("يرجى تصحيح الأخطاء في النموذج");
        return View(customer);
      }

      try
      {
        await _customerServices.AddCustomerAsync(customer);
        _toast.AddSuccessToastMessage("تم إضافة العميل بنجاح");
        return RedirectToAction(nameof(Index));
      }
      catch
      {
        _toast.AddErrorToastMessage("فشل إضافة العميل");
        return View(customer);
      }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
      try
      {
        var customer = await _customerServices.GetByIdAsync(id);
        if(customer==null)
        {
          _toast.AddWarningToastMessage("العميل غير موجود");
          return RedirectToAction(nameof(Index));
        }

        return View(customer);
      }
      catch
      {
        _toast.AddErrorToastMessage("حدث خطأ أثناء تحميل بيانات العميل");
        return RedirectToAction(nameof(Index));
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Customer customer)
    {
      if(!ModelState.IsValid)
      {
        _toast.AddInfoToastMessage("يرجى تصحيح الأخطاء في النموذج");
        return View(customer);
      }

      try
      {
        if(string.IsNullOrWhiteSpace(customer.PasswordHash))
        {
          var existing = await _customerServices.GetByIdAsync(customer.Id);
          if(existing!=null)
            customer.PasswordHash=existing.PasswordHash;
        }

        await _customerServices.UpdateCustomerAsync(customer);
        _toast.AddSuccessToastMessage("تم تحديث بيانات العميل بنجاح");
        return RedirectToAction(nameof(Index));
      }
      catch
      {
        _toast.AddErrorToastMessage("فشل تحديث بيانات العميل");
        return View(customer);
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
      try
      {
        var result = await _customerServices.DeleteCustomerAsync(id);

        if(!result)
          _toast.AddWarningToastMessage("لا يمكن حذف العميل (قد يكون له عقود نشطة)");
        else
          _toast.AddSuccessToastMessage("تم حذف العميل بنجاح");
      }
      catch
      {
        _toast.AddErrorToastMessage("فشل حذف العميل");
      }

      return RedirectToAction(nameof(Index));
    }
  }
}
