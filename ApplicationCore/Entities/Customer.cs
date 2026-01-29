namespace ApplicationCore.Entities
{
  public class Customer:EntityBase
  {
    public string Name { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string Phone { get; set; } = null!;
    public string DrivingLicenseNumber { get; set; } = null!;
    public DateTime LicenseExpiryDate { get; set; }

    public ICollection<RentalContract> RentalContracts { get; set; } = new List<RentalContract>();
  }

}
