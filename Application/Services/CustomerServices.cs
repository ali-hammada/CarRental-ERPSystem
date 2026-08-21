using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public interface ICustomerServices
    {
        Task<IEnumerable<Customer>> GetAllCustomersAsync();
        Task<Customer?> GetByIdAsync(int id);
        Task AddCustomerAsync(Customer customer);
        Task UpdateCustomerAsync(Customer customer);
        Task<bool> DeleteCustomerAsync(int id);
    }

    public class CustomerServices : ICustomerServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public CustomerServices(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task<IEnumerable<Customer>> GetAllCustomersAsync()
        {
            return await _unitOfWork.Customer.GetAll().ToListAsync();
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Customer.GetByIdAsync(id);
        }

        public async Task AddCustomerAsync(Customer customer)
        {
            customer.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Customer.AddAsync(customer);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                null,
                "Staff User",
                "Register Customer",
                "Customers",
                $"Registered new customer profile '{customer.Name}' ({customer.Phone}, Lic: {customer.DrivingLicenseNumber})."
            );
        }

        public async Task UpdateCustomerAsync(Customer customer)
        {
            var existing = await _unitOfWork.Customer.GetByIdAsync(customer.Id);
            if (existing == null)
            {
                throw new Exception("Customer standard record not found.");
            }
            existing.Name = customer.Name;
            existing.Email = customer.Email;
            existing.Phone = customer.Phone;
            existing.DrivingLicenseNumber = customer.DrivingLicenseNumber;
            existing.LicenseExpiryDate = customer.LicenseExpiryDate;
            existing.Address = customer.Address;
            existing.NationalId = customer.NationalId;
            existing.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Customer.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                null,
                "Staff User",
                "Update Customer",
                "Customers",
                $"Updated customer profile '{customer.Name}' ({customer.Phone})."
            );
        }

        public async Task<bool> DeleteCustomerAsync(int id)
        {
            var customer = await _unitOfWork.Customer.GetByIdAsync(id);
            if (customer == null) return false;

            customer.IsDeleted = true;
            customer.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Customer.Update(customer);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                null,
                "Staff User",
                "Delete Customer",
                "Customers",
                $"Soft-deleted customer profile '{customer.Name}'."
            );
            return true;
        }
    }
}
