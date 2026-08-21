using Application.Services.DTOs;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
    public interface IAuthenticationServices
    {
        Task<(bool success, string message)> RegisterAsync(RegisterDto dto);
        Task<(bool success, string message, Employee? employee)> LogInAsync(LoginDto dto);
    }

    public class AuthenticationServices : IAuthenticationServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public AuthenticationServices(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task<(bool success, string message)> RegisterAsync(RegisterDto dto)
        {
            var existingEmployee = await _unitOfWork.Employee.GetByEmailAsync(dto.Email);
            if (existingEmployee != null)
                return (false, "An employee account with this email address already exists.");

            var allEmployees = await _unitOfWork.Employee.GetAllAsync();
            string role = !string.IsNullOrWhiteSpace(dto.Role) 
                ? dto.Role 
                : (allEmployees.Any() ? "Employee" : "Admin");

            var employee = new Employee
            {
                FullName = dto.Name,
                Email = dto.Email,
                Phone = dto.Phone,
                Role = role,
                IsActive = true
            };

            var hasher = new PasswordHasher<Employee>();
            employee.PasswordHash = hasher.HashPassword(employee, dto.Password);

            await _unitOfWork.Employee.AddAsync(employee);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                employee.Id,
                employee.FullName,
                "User Registration",
                "Auth",
                $"Created new account '{employee.FullName}' ({employee.Email}) with role '{role}'."
            );

            return (true, "Registration successful.");
        }

        public async Task<(bool success, string message, Employee? employee)> LogInAsync(LoginDto dto)
        {
            var employee = await _unitOfWork.Employee.GetByEmailAsync(dto.Email);
            if (employee == null)
                return (false, "Invalid email address or password.", null);

            if (!employee.IsActive)
                return (false, "This user account is inactive. Please contact system manager.", null);

            if (string.IsNullOrEmpty(employee.PasswordHash))
                return (false, "Account password hash is missing.", null);

            var hasher = new PasswordHasher<Employee>();
            try
            {
                var result = hasher.VerifyHashedPassword(employee, employee.PasswordHash, dto.Password);

                if (result == PasswordVerificationResult.Failed)
                    return (false, "Invalid email address or password.", null);

                if (result == PasswordVerificationResult.SuccessRehashNeeded)
                {
                    employee.PasswordHash = hasher.HashPassword(employee, dto.Password);
                    _unitOfWork.Employee.Update(employee);
                    await _unitOfWork.SaveChangesAsync();
                }

                await _auditLogService.LogActionAsync(
                    employee.Id,
                    employee.FullName,
                    "User Authentication",
                    "Auth",
                    $"User '{employee.FullName}' logged in successfully into system."
                );

                return (true, "Login successful.", employee);
            }
            catch (Exception ex)
            {
                return (false, $"Authentication error: {ex.Message}", null);
            }
        }
    }
}