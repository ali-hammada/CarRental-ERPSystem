using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public interface IEmployeeServices
    {
        Task<IEnumerable<Employee>> GetAllAsync();
        Task<Employee?> GetByIdAsync(int id);
        Task AddAsync(Employee employee);
        Task UpdateAsync(Employee employee);
        Task<bool> DeleteAsync(int id);
        Task<Employee?> GetByEmailAsync(string email);
    }

    public class EmployeeServices : IEmployeeServices
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmployeeServices(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Employee>> GetAllAsync()
        {
            return await _unitOfWork.Employee.GetAll().ToListAsync();
        }

        public async Task<Employee?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Employee.GetByIdAsync(id);
        }

        public async Task<Employee?> GetByEmailAsync(string email)
        {
            return await _unitOfWork.Employee
                .GetAll()
                .FirstOrDefaultAsync(e => e.Email == email);
        }

        public async Task AddAsync(Employee employee)
        {
            await _unitOfWork.Employee.AddAsync(employee);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task UpdateAsync(Employee employee)
        {
            var existing = await GetByIdAsync(employee.Id);
            if (existing == null) throw new Exception("Employee not found");

            existing.FullName = employee.FullName;
            existing.Email = employee.Email;
            existing.Phone = employee.Phone;
            existing.Role = employee.Role;
            existing.IsActive = employee.IsActive;
            existing.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(employee.PasswordHash))
                existing.PasswordHash = employee.PasswordHash;

            _unitOfWork.Employee.Update(existing);
            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var emp = await GetByIdAsync(id);
            if (emp == null) return false;

            emp.IsDeleted = true;
            emp.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Employee.Update(emp);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
