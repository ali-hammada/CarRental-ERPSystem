using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace InFrastructure.Data
{
  public class AppDbContext:DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Car> Cars { get; set; } = null!;
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<RentalContract> RentalContracts { get; set; } = null!;
    public DbSet<Payment> Payments { get; set; } = null!;
    public DbSet<Employees> Employee { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);


      modelBuilder.Entity<RentalContract>()
           .HasOne(r => r.Car)
           .WithMany(c => c.RentalContracts)
           .HasForeignKey(r => r.CarId)
           .OnDelete(DeleteBehavior.Restrict);


      modelBuilder.Entity<RentalContract>()
          .HasOne(r => r.Employee)
          .WithMany(e => e.RentalContracts)
          .HasForeignKey(r => r.EmployeeId)
          .OnDelete(DeleteBehavior.Restrict);


      modelBuilder.Entity<Payment>()
          .HasOne(p => p.RentalContract)
          .WithMany(r => r.Payments)
          .HasForeignKey(p => p.RentalContractId)
          .OnDelete(DeleteBehavior.Cascade);
    }
  }
}
