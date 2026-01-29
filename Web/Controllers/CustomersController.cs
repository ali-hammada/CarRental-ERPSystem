using Application.Services.Interfaces;
using ApplicationCore.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
  [Authorize]
  public class CustomersController:Controller
  {
    private readonly ICustomerServices _customerServices;

    public CustomersController(ICustomerServices customerServices)
    {
      _customerServices=customerServices;
    }


    public async Task<IActionResult> Index()
    {
      try
      {
        var customers = await _customerServices.GetAllCustomersAsync();
        return View(customers);
      }
      catch(Exception ex)
      {
        TempData["Error"]="حدث خطأ أثناء تحميل قائمة العملاء: "+ex.Message;
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
        TempData["Error"]="يرجى تصحيح الأخطاء في النموذج.";
        return View(customer);
      }

      try
      {
        await _customerServices.AddCustomerAsync(customer);
        TempData["Success"]="تم إضافة العميل بنجاح!";
        return RedirectToAction(nameof(Index));
      }
      catch(Exception ex)
      {
        ModelState.AddModelError("","حدث خطأ أثناء إضافة العميل: "+ex.Message);
        TempData["Error"]="فشل إضافة العميل: "+ex.Message;
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
          TempData["Error"]="العميل غير موجود!";
          return RedirectToAction(nameof(Index));
        }

        return View(customer);
      }
      catch(Exception ex)
      {
        TempData["Error"]="حدث خطأ أثناء تحميل بيانات العميل: "+ex.Message;
        return RedirectToAction(nameof(Index));
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Customer customer)
    {
      if(!ModelState.IsValid)
      {
        TempData["Error"]="يرجى تصحيح الأخطاء في النموذج.";
        return View(customer);
      }

      try
      {

        if(string.IsNullOrWhiteSpace(customer.PasswordHash))
        {
          var existingCustomer = await _customerServices.GetByIdAsync(customer.Id);
          if(existingCustomer!=null)
          {
            customer.PasswordHash=existingCustomer.PasswordHash;
          }
        }

        // تعديل العميل
        await _customerServices.UpdateCustomerAsync(customer);
        TempData["Success"]="تم تحديث بيانات العميل بنجاح!";
        return RedirectToAction(nameof(Index));
      }
      catch(Exception ex)
      {
        ModelState.AddModelError("","حدث خطأ أثناء تحديث العميل: "+ex.Message);
        TempData["Error"]="فشل تحديث العميل: "+ex.Message;
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
        {
          TempData["Error"]="العميل لا يمكن حذفه (قد يكون له عقود نشطة)!";
        }
        else
        {
          TempData["Success"]="تم حذف العميل بنجاح!";
        }
      }
      catch(Exception ex)
      {
        TempData["Error"]="فشل حذف العميل: "+ex.Message;
      }
      return RedirectToAction(nameof(Index));
    }



  }

}

