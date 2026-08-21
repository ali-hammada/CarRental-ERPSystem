using Application.DTOs;
using ApplicationCore.Enums;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Providers
{
    public interface ICustomerProvider
    {
        Task<List<CustomerListDto>> GetAllCustomersAsync();
        Task<CustomerListDto?> GetCustomerByIdAsync(int id);
    }

    public class CustomerProvider : ICustomerProvider
    {
        private readonly AppDbContext _context;

        public CustomerProvider(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CustomerListDto>> GetAllCustomersAsync()
        {
            return await _context.Customers
                .AsNoTracking()
                .Select(c => new CustomerListDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.Phone,
                    DrivingLicenseNumber = c.DrivingLicenseNumber,
                    LicenseExpiryDate = c.LicenseExpiryDate,
                    Address = c.Address,
                    ActiveRentalsCount = c.RentalContracts.Count(r => r.Status == RentalContractStatus.Open)
                })
                .ToListAsync();
        }

        public async Task<CustomerListDto?> GetCustomerByIdAsync(int id)
        {
            return await _context.Customers
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CustomerListDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Phone = c.Phone,
                    DrivingLicenseNumber = c.DrivingLicenseNumber,
                    LicenseExpiryDate = c.LicenseExpiryDate,
                    Address = c.Address,
                    ActiveRentalsCount = c.RentalContracts.Count(r => r.Status == RentalContractStatus.Open)
                })
                .FirstOrDefaultAsync();
        }
    }
}
