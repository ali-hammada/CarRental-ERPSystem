using Application.Services;
using ApplicationCore.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Web.ViewModels;

namespace Web.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    public class EmployeesController : Controller
    {
        private readonly IEmployeeServices _employeeServices;

        public EmployeesController(IEmployeeServices employeeServices)
        {
            _employeeServices = employeeServices;
        }

        public async Task<IActionResult> Index()
        {
            var employees = await _employeeServices.GetAllAsync();
            var model = employees.Select(e => new EmployeeVM
            {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email,
                Phone = e.Phone,
                Role = e.Role,
                IsActive = e.IsActive
            }).ToList();

            return View(model);
        }

        [HttpGet]
        public IActionResult Create()
        {
            return View("CreateEdit", new EmployeeVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(EmployeeVM vm)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join("<br>", errors) });
                }
                return View("CreateEdit", vm);
            }

            var existing = await _employeeServices.GetByEmailAsync(vm.Email);
            if (existing != null)
            {
                ModelState.AddModelError("Email", "Email already exists.");
                if (isAjax) return Json(new { success = false, message = "Email already exists." });
                return View("CreateEdit", vm);
            }

            var employee = new Employee
            {
                FullName = vm.FullName,
                Email = vm.Email,
                Phone = vm.Phone,
                Role = vm.Role,
                IsActive = vm.IsActive,
            };

            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                var hasher = new PasswordHasher<Employee>();
                employee.PasswordHash = hasher.HashPassword(employee, vm.Password);
            }

            await _employeeServices.AddAsync(employee);

            if (isAjax) return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var employee = await _employeeServices.GetByIdAsync(id);
            if (employee == null)
            {
                return RedirectToAction(nameof(Index));
            }
            var vm = new EmployeeVM
            {
                Id = employee.Id,
                FullName = employee.FullName,
                Email = employee.Email,
                Phone = employee.Phone,
                Role = employee.Role,
                IsActive = employee.IsActive,
            };

            return View("CreateEdit", vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EmployeeVM vm)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            ModelState.Remove("PasswordHash");

            if (!ModelState.IsValid)
            {
                if (isAjax)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage);
                    return Json(new { success = false, message = string.Join("<br>", errors) });
                }
                return View("CreateEdit", vm);
            }

            var employee = await _employeeServices.GetByIdAsync(vm.Id);
            if (employee == null)
            {
                if (isAjax) return Json(new { success = false, message = "Employee not found." });
                return RedirectToAction(nameof(Index));
            }

            var existing = await _employeeServices.GetByEmailAsync(vm.Email);
            if (existing != null && existing.Id != vm.Id)
            {
                ModelState.AddModelError("Email", "Email address is already in use.");
                if (isAjax) return Json(new { success = false, message = "Email address is already in use." });
                return View("CreateEdit", vm);
            }

            employee.FullName = vm.FullName;
            employee.Email = vm.Email;
            employee.Phone = vm.Phone;
            employee.Role = vm.Role;
            employee.IsActive = vm.IsActive;

            if (!string.IsNullOrWhiteSpace(vm.Password))
            {
                var hasher = new PasswordHasher<Employee>();
                employee.PasswordHash = hasher.HashPassword(employee, vm.Password);
            }

            await _employeeServices.UpdateAsync(employee);

            if (isAjax) return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var employee = await _employeeServices.GetByIdAsync(id);
            if (employee == null)
            {
                if (isAjax) return Json(new { success = false, message = "Employee not found." });
                return RedirectToAction(nameof(Index));
            }

            await _employeeServices.DeleteAsync(id);
            if (isAjax) return Json(new { success = true, redirectUrl = Url.Action(nameof(Index)) });
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Permissions()
        {
            var employees = await _employeeServices.GetAllAsync();
            var model = employees.Select(e => new EmployeeVM
            {
                Id = e.Id,
                FullName = e.FullName,
                Email = e.Email,
                Phone = e.Phone,
                Role = string.IsNullOrWhiteSpace(e.Role) ? "Employee" : e.Role,
                IsActive = e.IsActive
            }).ToList();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateRole(int id, string role)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var employee = await _employeeServices.GetByIdAsync(id);
            if (employee == null)
            {
                if (isAjax) return Json(new { success = false, message = "User account not found." });
                return RedirectToAction(nameof(Permissions));
            }

            employee.Role = role;
            await _employeeServices.UpdateAsync(employee);

            if (isAjax) return Json(new { success = true, redirectUrl = Url.Action(nameof(Permissions)) });
            return RedirectToAction(nameof(Permissions));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleStatus(int id)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            var employee = await _employeeServices.GetByIdAsync(id);
            if (employee == null)
            {
                if (isAjax) return Json(new { success = false, message = "User account not found." });
                return RedirectToAction(nameof(Permissions));
            }

            employee.IsActive = !employee.IsActive;
            await _employeeServices.UpdateAsync(employee);

            if (isAjax) return Json(new { success = true, redirectUrl = Url.Action(nameof(Permissions)) });
            return RedirectToAction(nameof(Permissions));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> QuickResetPassword(int id, string newPassword)
        {
            bool isAjax = Request.Headers["X-Requested-With"] == "XMLHttpRequest";
            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                if (isAjax) return Json(new { success = false, message = "Password must be at least 6 characters long." });
                return RedirectToAction(nameof(Permissions));
            }

            var employee = await _employeeServices.GetByIdAsync(id);
            if (employee == null)
            {
                if (isAjax) return Json(new { success = false, message = "User account not found." });
                return RedirectToAction(nameof(Permissions));
            }

            var hasher = new PasswordHasher<Employee>();
            employee.PasswordHash = hasher.HashPassword(employee, newPassword);
            await _employeeServices.UpdateAsync(employee);

            if (isAjax) return Json(new { success = true, redirectUrl = Url.Action(nameof(Permissions)) });
            return RedirectToAction(nameof(Permissions));
        }

        [HttpPost]
        public async Task<IActionResult> CheckEmailExists(string email)
        {
            var exists = await _employeeServices.GetByEmailAsync(email);
            return Json(new { exists = exists != null });
        }
    }
}
