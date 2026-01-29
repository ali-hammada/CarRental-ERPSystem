using Application.Services.Interfaces;
using ApplicationCore.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Web.Controllers
{
  [Authorize]
  public class EmployeesController:Controller
  {
    private readonly IEmployeeServices _employeeServices;

    public EmployeesController(IEmployeeServices employeeServices)
    {
      _employeeServices=employeeServices;
    }

    public async Task<IActionResult> Index()
    {
      var employees = await _employeeServices.GetAllAsync();
      return View(employees);
    }

    [HttpGet]
    public IActionResult Create()
    {
      return View("Upsert",new Employees());
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
      var employee = await _employeeServices.GetByIdAsync(id);
      if(employee==null)
      {
        TempData["Error"]="Employee not found!";
        return RedirectToAction("Index");
      }
      return View("Upsert",employee);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Employees employee)
    {
      if(!ModelState.IsValid)
      {
        return View("Upsert",employee);
      }

      try
      {
        // Check for duplicate email
        var existingByEmail = await _employeeServices.GetByEmailAsync(employee.Email);
        if(existingByEmail!=null)
        {
          ModelState.AddModelError("Email","Email already exists!");
          TempData["Error"]="Email already exists!";
          return View("Upsert",employee);
        }

        await _employeeServices.AddAsync(employee);
        TempData["Success"]="Employee added successfully!";
        return RedirectToAction("Index");
      }
      catch(Exception ex)
      {
        ModelState.AddModelError("",ex.Message);
        TempData["Error"]="Failed to add employee: "+ex.Message;
        return View("Upsert",employee);
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Employees employee)
    {
      if(!ModelState.IsValid)
      {
        return View("Upsert",employee);
      }

      try
      {
        // Check email (if changed)
        var existingByEmail = await _employeeServices.GetByEmailAsync(employee.Email);
        if(existingByEmail!=null&&existingByEmail.Id!=employee.Id)
        {
          ModelState.AddModelError("Email","Email already exists!");
          TempData["Error"]="Email already exists!";
          return View("Upsert",employee);
        }

        // If password is provided, update it
        if(!string.IsNullOrWhiteSpace(employee.PasswordHash))
        {
          // Password will be hashed in the service layer
        }
        else
        {
          // Keep existing password - retrieve it from database
          var existingEmployee = await _employeeServices.GetByIdAsync(employee.Id);
          if(existingEmployee!=null)
          {
            employee.PasswordHash=existingEmployee.PasswordHash;
          }
        }

        await _employeeServices.UpdateAsync(employee);
        TempData["Success"]="Employee updated successfully!";
        return RedirectToAction("Index");
      }
      catch(Exception ex)
      {
        ModelState.AddModelError("",ex.Message);
        TempData["Error"]="Failed to update employee: "+ex.Message;
        return View("Upsert",employee);
      }
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
      var employee = await _employeeServices.GetByIdAsync(id);
      if(employee==null)
      {
        TempData["Error"]="Employee not found!";
        return RedirectToAction("Index");
      }
      return View(employee);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
      try
      {
        await _employeeServices.DeleteAsync(id);
        TempData["Success"]="Employee deleted successfully!";
      }
      catch(Exception ex)
      {
        TempData["Error"]="Failed to delete employee: "+ex.Message;
      }
      return RedirectToAction("Index");
    }

    // AJAX methods for checking duplicates
    [HttpPost]
    public async Task<IActionResult> CheckEmailExists(string email)
    {
      var exists = await _employeeServices.GetByEmailAsync(email);
      return Json(new { exists = exists!=null });
    }


  }
}