using ApplicationCore.Entities;
using Microsoft.EntityFrameworkCore;

namespace InFrastructure.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Car> Cars { get; set; } = null!;
        public DbSet<Customer> Customers { get; set; } = null!;
        public DbSet<Employee> Employees { get; set; } = null!;
        public DbSet<RentalContract> RentalContracts { get; set; } = null!;
        public DbSet<Payment> Payments { get; set; } = null!;
        public DbSet<CarCategory> CarCategories { get; set; } = null!;
        public DbSet<MaintenanceLog> MaintenanceLogs { get; set; } = null!;
        public DbSet<Invoice> Invoices { get; set; } = null!;
        public DbSet<CarLocationLog> CarLocationLogs { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;
        public DbSet<CarSaleContract> CarSaleContracts { get; set; } = null!;
        public DbSet<SaleInstallment> SaleInstallments { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Global Soft Delete Query Filters
            modelBuilder.Entity<Car>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Customer>().HasQueryFilter(c => !c.IsDeleted);
            modelBuilder.Entity<Employee>().HasQueryFilter(e => !e.IsDeleted);
            modelBuilder.Entity<RentalContract>().HasQueryFilter(r => !r.IsDeleted);
            modelBuilder.Entity<Payment>().HasQueryFilter(p => !p.IsDeleted);
            modelBuilder.Entity<Invoice>().HasQueryFilter(i => !i.IsDeleted);
            modelBuilder.Entity<MaintenanceLog>().HasQueryFilter(m => !m.IsDeleted);
            modelBuilder.Entity<CarLocationLog>().HasQueryFilter(l => !l.IsDeleted);
            modelBuilder.Entity<AuditLog>().HasQueryFilter(a => !a.IsDeleted);
            modelBuilder.Entity<CarSaleContract>().HasQueryFilter(s => !s.IsDeleted);
            modelBuilder.Entity<SaleInstallment>().HasQueryFilter(i => !i.IsDeleted);

            // Precision for Decimals
            foreach (var property in modelBuilder.Model.GetEntityTypes()
                .SelectMany(t => t.GetProperties())
                .Where(p => p.ClrType == typeof(decimal) || p.ClrType == typeof(decimal?)))
            {
                property.SetPrecision(18);
                property.SetScale(2);
            }

            // Indexes
            modelBuilder.Entity<Car>()
                .HasIndex(c => c.PlateNumber)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.DrivingLicenseNumber)
                .IsUnique();

            modelBuilder.Entity<Customer>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<Employee>()
                .HasIndex(e => e.Email)
                .IsUnique();

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.InvoiceNumber)
                .IsUnique();

            modelBuilder.Entity<RentalContract>()
                .HasIndex(r => new { r.CarId, r.StartDate, r.EndDate });

            // Relationships
            modelBuilder.Entity<RentalContract>()
                .HasOne(r => r.Car)
                .WithMany(c => c.RentalContracts)
                .HasForeignKey(r => r.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<RentalContract>()
                .HasOne(r => r.Customer)
                .WithMany(c => c.RentalContracts)
                .HasForeignKey(r => r.CustomerId)
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

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.RentalContract)
                .WithMany(r => r.Invoices)
                .HasForeignKey(i => i.RentalContractId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<MaintenanceLog>()
                .HasOne(m => m.Car)
                .WithMany(c => c.MaintenanceLogs)
                .HasForeignKey(m => m.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<CarLocationLog>()
                .HasOne(l => l.Car)
                .WithMany(c => c.LocationLogs)
                .HasForeignKey(l => l.CarId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Car>()
                .HasOne(c => c.Category)
                .WithMany(cat => cat.Cars)
                .HasForeignKey(c => c.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<CarSaleContract>()
                .HasOne(s => s.Car)
                .WithMany(c => c.SaleContracts)
                .HasForeignKey(s => s.CarId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CarSaleContract>()
                .HasOne(s => s.Customer)
                .WithMany()
                .HasForeignKey(s => s.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<CarSaleContract>()
                .HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<SaleInstallment>()
                .HasOne(i => i.SaleContract)
                .WithMany(s => s.Installments)
                .HasForeignKey(i => i.SaleContractId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
