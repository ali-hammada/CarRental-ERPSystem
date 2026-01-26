using ApplicationCore.Entities;

namespace Application.Services.Interfaces
{
  public interface ITokenServices
  {
    string GenerateToken(Customer customer);
  }
}
