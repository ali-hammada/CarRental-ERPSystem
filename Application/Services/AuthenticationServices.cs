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
    private readonly ICustomerRepository _customerRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ITokenServices _tokenService;

    public AuthenticationServices(ICustomerRepository customerRepository,IUnitOfWork unitOfWork,ITokenServices tokenService)
    {
      _customerRepository=customerRepository;
      _tokenService=tokenService;
      _unitOfWork=unitOfWork;

    }

    public async Task<(bool success, string message)> RegisterAsync(RegisterDto dto)
    {
      var ExistingCustomer = await _customerRepository.GetByEmail(dto.Email);
      if(ExistingCustomer==null)
      {
        var newCustomer = new Customer
        {
          Name=dto.Name,
          Email=dto.Email,
          Phone=dto.Phone,
          DrivingLicenseNumber=dto.DrivingLicenseNumber??"",
          LicenseExpiryDate=dto.LicenseExpiryDate,

        };
        var hasher = new PasswordHasher<Customer>();
        string hashedPassword = hasher.HashPassword(newCustomer,dto.Password);
        newCustomer.PasswordHash=hashedPassword;

        await _customerRepository.AddAsync(newCustomer);
        await _unitOfWork.SaveChangesAsync();

        return (true, "Registration successful.");
      }
      return (false, "Customer with this email already exists.");
    }
    public async Task<(bool success, string message, Customer? customer)> LogInAsync(LoginDto dto)
    {
      var customer = await _customerRepository.GetByEmail(dto.Email);
      if(customer==null)
        return (false, "Customer Does not Exist Yet.", null);
      var hasher = new PasswordHasher<Customer>();
      var result = hasher.VerifyHashedPassword(customer,customer.PasswordHash,dto.Password);

      if(result==PasswordVerificationResult.Failed)
        return (false, "Invalid Email or Password", null);
      var token = _tokenService.GenerateToken(customer);
      return (true, token, customer);


    }


  }
}
