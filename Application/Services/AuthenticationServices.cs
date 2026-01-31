using Application.Services.DTO_s;
using Application.Services.Interfaces;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Repositories;
using Microsoft.AspNetCore.Identity;

namespace Application.Services
{
  public class AuthenticationServices:IAuthenticationServices
  {
    private readonly IEmployeeRepository _employeeRepository;
    private readonly IUnitOfWork _unitOfWork;

    public AuthenticationServices(
        IEmployeeRepository employeeRepository,
        IUnitOfWork unitOfWork)
    {
      _employeeRepository=employeeRepository;
      _unitOfWork=unitOfWork;
    }

    public async Task<(bool success, string message)> RegisterAsync(RegisterDto dto)
    {
      var existingEmployee = await _employeeRepository.GetByEmailAsync(dto.Email);
      if(existingEmployee!=null)
        return (false, "Employee with this email already exists.");

      // شوف لو فيه أي موظف موجود - لو لا فالأول سيكون Admin
      var allEmployees = await _employeeRepository.GetAllAsync();
      string role = allEmployees.Any() ? "Employee" : "Admin";

      var employee = new Employees
      {
        FullName=dto.Name,
        Email=dto.Email,
        Phone=dto.Phone,
        Role=role
      };

      var hasher = new PasswordHasher<Employees>();
      employee.PasswordHash=hasher.HashPassword(employee,dto.Password);
      await _employeeRepository.AddAsync(employee);
      await _unitOfWork.SaveChangesAsync();
      return (true, "Registration successful.");
    }

    public async Task<(bool success, string message, Employees? employee)> LogInAsync(LoginDto dto)
    {
      var employee = await _employeeRepository.GetByEmailAsync(dto.Email);
      if(employee==null)
        return (false, "Employee does not exist.", null);

      // مؤقت: شوف إيه القيمة الفعلية
      if(string.IsNullOrEmpty(employee.PasswordHash))
        return (false, $"PasswordHash is NULL or Empty for {employee.Email}", null);

      var hasher = new PasswordHasher<Employees>();
      try
      {
        var result = hasher.VerifyHashedPassword(
            employee,
            employee.PasswordHash,
            dto.Password);

        if(result==PasswordVerificationResult.Failed)
          return (false, "Invalid email or password.", null);

        if(result==PasswordVerificationResult.SuccessRehashNeeded)
        {
          employee.PasswordHash=hasher.HashPassword(employee,dto.Password);
          _employeeRepository.Update(employee);
          await _unitOfWork.SaveChangesAsync();
        }

        return (true, "Login successful", employee);
      }
      catch(Exception ex)
      {
        // مؤقت: شوف الخطأ الفعلي
        return (false, $"Error: {ex.Message} | Hash: {employee.PasswordHash?.Substring(0,Math.Min(20,employee.PasswordHash.Length))}...", null);
      }
    }
  }

}