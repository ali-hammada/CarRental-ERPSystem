using Application.Services.Interfaces;
using ApplicationCore.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using NToastNotify;
using Web.ViewModels;

namespace Web.Controllers
{
  [Authorize(Policy = "AdminOnly")]
  public class EmployeesController:Controller
  {
    private readonly IEmployeeServices _employeeServices;
    private readonly IToastNotification _toast;

    public EmployeesController(IEmployeeServices employeeServices,IToastNotification toast)
    {
      _employeeServices=employeeServices;
      _toast=toast;
    }
    public async Task<IActionResult> Index()
    {
      var employees = await _employeeServices.GetAllAsync();
      var model = employees.Select(e => new EmployeeVM
      {
        Id=e.Id,
        FullName=e.FullName,
        Email=e.Email,
        Phone=e.Phone,
        Role=e.Role,
        IsActive=e.IsActive
      }).ToList();

      return View(model);
    }
    [HttpGet]
    public IActionResult Create()
    {
      return View(new EmployeeVM());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(EmployeeVM vm)
    {
      if(!ModelState.IsValid)
      {
        _toast.AddInfoToastMessage("Please fix validation errors");
        return View(vm);
      }

      var existing = await _employeeServices.GetByEmailAsync(vm.Email);
      if(existing!=null)
      {
        ModelState.AddModelError("Email","Email already exists");
        _toast.AddWarningToastMessage("Email already exists");
        return View(vm);
      }

      var employee = new Employees
      {
        FullName=vm.FullName,
        Email=vm.Email,
        Phone=vm.Phone,
        Role=vm.Role,
        IsActive=vm.IsActive,
      };


      var hasher = new PasswordHasher<Employees>();
      employee.PasswordHash=hasher.HashPassword(employee,vm.Password);

      await _employeeServices.AddAsync(employee);
      _toast.AddSuccessToastMessage("Employee added successfully");
      return RedirectToAction(nameof(Index));
    }
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
      var employee = await _employeeServices.GetByIdAsync(id);
      if(employee==null)
      {
        _toast.AddErrorToastMessage("Employee not found");
        return RedirectToAction(nameof(Index));
      }
      var vm = new EmployeeVM
      {
        Id=employee.Id,
        FullName=employee.FullName,
        Email=employee.Email,
        Phone=employee.Phone,
        Role=employee.Role,
        IsActive=employee.IsActive,

      };

      return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EmployeeVM vm)
    {
      ModelState.Remove("PasswordHash");

      if(!ModelState.IsValid)
      {
        ShowValidationErrors(vm);
        return View(vm);
      }

      var employee = await _employeeServices.GetByIdAsync(vm.Id);
      if(employee==null)
      {
        _toast.AddErrorToastMessage("Employee not found");
        return RedirectToAction(nameof(Index));
      }

      var existing = await _employeeServices.GetByEmailAsync(vm.Email);
      if(existing!=null&&existing.Id!=vm.Id)
      {
        ModelState.AddModelError("Email","Email already exists");
        _toast.AddWarningToastMessage("Email already exists");
        return View(vm);
      }

      employee.FullName=vm.FullName;
      employee.Email=vm.Email;
      employee.Phone=vm.Phone;
      employee.Role=vm.Role;
      employee.IsActive=vm.IsActive;


      if(!string.IsNullOrWhiteSpace(vm.Password))
      {
        var hasher = new PasswordHasher<Employees>();
        employee.PasswordHash=hasher.HashPassword(employee,vm.Password);
      }
      await _employeeServices.UpdateAsync(employee);
      _toast.AddSuccessToastMessage($"{employee.FullName} updated successfully");
      return RedirectToAction(nameof(Index));
    }


    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
      var employee = await _employeeServices.GetByIdAsync(id);
      if(employee==null)
      {
        _toast.AddErrorToastMessage("Employee not found");
        return RedirectToAction(nameof(Index));
      }

      await _employeeServices.DeleteAsync(id);
      _toast.AddSuccessToastMessage("Employee deleted successfully");
      return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    public async Task<IActionResult> CheckEmailExists(string email)
    {
      var exists = await _employeeServices.GetByEmailAsync(email);
      return Json(new { exists = exists!=null });
    }
    private void ShowValidationErrors(EmployeeVM vm)
    {
      var errors = ModelState.Values
                  .SelectMany(v => v.Errors)
                  .Select(e => e.ErrorMessage)
                  .ToList();

      if(errors.Any())
      {
        var message = string.Join("<br>",errors);
        _toast.AddErrorToastMessage(message);
      }
      else
      {
        _toast.AddInfoToastMessage("Please fix validation errors");
      }
    }
  }
}
