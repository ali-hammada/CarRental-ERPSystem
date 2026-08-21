namespace ApplicationCore.Entities
{
    public class Employee : EntityBase
    {
        public string FullName { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string Role { get; set; } = "Employee";
        public bool IsActive { get; set; } = true;

        public ICollection<RentalContract> RentalContracts { get; set; } = new List<RentalContract>();
    }
}
