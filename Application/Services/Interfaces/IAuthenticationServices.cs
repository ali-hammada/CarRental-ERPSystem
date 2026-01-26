using Application.Services.DTO_s;
using ApplicationCore.Entities;

namespace Application.Services.Interfaces
{
  public interface IAuthenticationServices
  {
    public Task<(bool success, string message)> RegisterAsync(RegisterDto dto);
    public Task<(bool success, string message, Customer? customer)> LogInAsync(LoginDto dto);
  }
}
