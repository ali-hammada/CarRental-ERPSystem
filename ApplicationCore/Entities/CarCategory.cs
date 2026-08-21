namespace ApplicationCore.Entities
{
    public class CarCategory : EntityBase
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public decimal DailyRateMultiplier { get; set; } = 1.0m;
        public decimal RequiredDeposit { get; set; } = 0;

        public ICollection<Car> Cars { get; set; } = new List<Car>();
    }
}
