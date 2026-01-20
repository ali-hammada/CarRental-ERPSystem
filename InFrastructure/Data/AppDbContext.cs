using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace InFrastructure.Data
{
  public class AppDbContext:DbContext
  {
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {


    }
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
      base.OnModelCreating(modelBuilder);
      //Car Entity 
      modelBuilder.Entity<Car>(Entity =>
      {
        Entity.HasKey(c => c.Id);
        Entity.Property(c => c.PlateNumber).IsRequired().HasMaxLength(20);
        Entity.HasIndex(c => c.PlateNumber).IsUnique();
        Entity.Property(c => c.Model).IsRequired().HasMaxLength(50);
        Entity.Property(c => c.PricePerDay).HasColumnType("decimal(10,2)");
        Entity.Property(c => c.Status).HasConversion<int>();

      }
      );
      //Customer Entity
      modelBuilder.Entity<Customer>(Entity =>
      {
        Entity.HasKey(c => c.Id);
        Entity.Property(c => c.FullName).IsRequired().HasMaxLength(100);
        Entity.Property(c => c.Phone).IsRequired().HasMaxLength(20);
        Entity.Property(c => c.DrivingLicenseNumber).IsRequired().HasMaxLength(50);
      });


      //RentalContract Entity
      modelBuilder.Entity<RentalContract>(Entity =>
      {
        Entity.HasKey(r => r.Id);
        Entity.Property(r => r.TotalPrice).HasColumnType("decimal(10,2)");
        Entity.HasOne(r => r.Car)
             .WithMany(c => c.RentalContracts)
             .HasForeignKey(r => r.CarId)
             .OnDelete(DeleteBehavior.Restrict);

        Entity.HasOne(r => r.Customer)
              .WithMany(c => c.RentalContracts)
              .HasForeignKey(r => r.CustomerId)
              .OnDelete(DeleteBehavior.Restrict);

      });
      //Payment Entity
      modelBuilder.Entity<Payment>(Entity =>
      {
        Entity.HasKey(p => p.Id);
        Entity.Property(p => p.Amount).HasColumnType("decimal(10,2)");
        Entity.Property(p => p.Method).IsRequired().HasMaxLength(50);
        Entity.HasOne(p => p.RentalContract).WithMany()
        .HasForeignKey(p => p.RentalContractId).OnDelete(DeleteBehavior.Cascade);

      });



    }
    public DbSet<Car> Cars { get; set; }
    public DbSet<Customer> Customers { get; set; }
    public DbSet<RentalContract> RentalContracts { get; set; }
    public DbSet<Payment> Payments { get; set; }


  }
}
