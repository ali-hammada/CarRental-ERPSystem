namespace Application.Services.DTO_s
{
  public class RegisterDto
  {
    public string Name { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
    public string ConfirmPassword { get; set; }
    public string? DrivingLicenseNumber { get; set; } = null!;
    public string Phone { get; set; }

    public DateTime LicenseExpiryDate { get; set; }
  }
}
