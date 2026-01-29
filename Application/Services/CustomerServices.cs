using Application.Services.Interfaces;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{

  public class CustomerServices:ICustomerServices

  {
    private readonly IUnitOfWork _unitOfWork;


    public CustomerServices(IUnitOfWork unitOfWork)
    {
      _unitOfWork=unitOfWork;

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
      await _unitOfWork.Customer.AddAsync(customer);
      await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateCustomerAsync(Customer customer)
    {
      var existingCustomer = await _unitOfWork.Customer.GetByIdAsync(customer.Id);
      if(existingCustomer==null)
      {
        throw new Exception("Customer not found!");
      }
      existingCustomer.Name=customer.Name;
      existingCustomer.Email=customer.Email;
      existingCustomer.Phone=customer.Phone;
      existingCustomer.DrivingLicenseNumber=customer.DrivingLicenseNumber;
      existingCustomer.LicenseExpiryDate=customer.LicenseExpiryDate;

      await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteCustomerAsync(int id)
    {
      var customer = await _unitOfWork.Customer.GetByIdAsync(id);
      if(customer==null) return false;

      _unitOfWork.Customer.Delete(customer);
      await _unitOfWork.SaveChangesAsync();
      return true;
    }

  }
}
