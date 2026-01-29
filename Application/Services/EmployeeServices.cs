using Application.Services.Interfaces;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;

public class EmployeeServices:IEmployeeServices
{
  private readonly IUnitOfWork _unitOfWork;

  public EmployeeServices(IUnitOfWork unitOfWork)
  {
    _unitOfWork=unitOfWork;
  }

  public async Task<IEnumerable<Employees>> GetAllAsync()
  {
    return await _unitOfWork.Employee.GetAll().ToListAsync();
  }

  public async Task<Employees?> GetByIdAsync(int id)
  {
    return await _unitOfWork.Employee.GetByIdAsync(id);
  }

  public async Task<Employees?> GetByEmailAsync(string email)
  {
    return await _unitOfWork.Employee
        .GetAll()
        .FirstOrDefaultAsync(e => e.Email==email);
  }

  public async Task AddAsync(Employees employee)
  {
    await _unitOfWork.Employee.AddAsync(employee);
    await _unitOfWork.SaveChangesAsync();
  }

  public async Task UpdateAsync(Employees employee)
  {
    var existing = await GetByIdAsync(employee.Id);
    if(existing==null) throw new Exception("Employee not found");

    existing.FullName=employee.FullName;
    existing.Email=employee.Email;
    existing.Phone=employee.Phone;
    existing.Role=employee.Role;
    existing.IsActive=employee.IsActive;

    await _unitOfWork.SaveChangesAsync();
  }

  public async Task<bool> DeleteAsync(int id)
  {
    var emp = await GetByIdAsync(id);
    if(emp==null) return false;

    _unitOfWork.Employee.Delete(emp);
    await _unitOfWork.SaveChangesAsync();
    return true;
  }
}
