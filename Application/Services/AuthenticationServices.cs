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

      var employee = new Employees
      {
        FullName=dto.Name,
        Email=dto.Email,
        Phone=dto.Phone,
        Role="Admin"
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

      var hasher = new PasswordHasher<Employees>();

      var result = hasher.VerifyHashedPassword(
          employee,
          employee.PasswordHash,
          dto.Password);

      if(result==PasswordVerificationResult.Failed)
        return (false, "Invalid email or password.", null);

      return (true, "Login successful", employee);
    }
  }
}
