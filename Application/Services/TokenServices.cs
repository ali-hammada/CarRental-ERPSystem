using Application.Services.Interfaces;
using ApplicationCore.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Application.Services
{
  public class TokenServices:ITokenServices
  {
    private readonly IConfiguration _config;
    public TokenServices(IConfiguration config)
    {
      _config=config;
    }
    public string GenerateToken(Customer customer)
    {

      var claims = new[]
           {
                new Claim(type: JwtRegisteredClaimNames.Sub, value: customer.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, customer.Email),
                new Claim(ClaimTypes.Name, customer.Name)
            };

      var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["Jwt:Key"]!));
      var creds = new SigningCredentials(key,SecurityAlgorithms.HmacSha256);

      var token = new JwtSecurityToken(
          _config["Jwt:Issuer"],
          _config["Jwt:Audience"],
          claims,
          expires: DateTime.Now.AddHours(2),
          signingCredentials: creds
      );

      return new JwtSecurityTokenHandler().WriteToken(token);


    }
  }
}
