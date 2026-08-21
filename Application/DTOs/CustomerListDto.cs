namespace Application.DTOs
{
    public class CustomerListDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string DrivingLicenseNumber { get; set; } = null!;
        public DateTime LicenseExpiryDate { get; set; }
        public string? Address { get; set; }
        public bool IsLicenseExpired => LicenseExpiryDate.Date < DateTime.UtcNow.Date;
        public int ActiveRentalsCount { get; set; }
    }
}
