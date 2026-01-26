using Application.Services.Interfaces;
using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
  public class CarServices:ICarServices
  {
    private readonly IUnitOfWork _unitOfWork;

    public CarServices(IUnitOfWork unitOfWork)
    {
      _unitOfWork=unitOfWork;
    }

    public async Task<IEnumerable<Car>> GetAvailableCarsAsync()
    {
      return await _unitOfWork.Cars.GetAvailableCars().ToListAsync();
    }

    public async Task<IEnumerable<Car>> GetAllCarsAsync()
    {
      return await _unitOfWork.Cars.GetAll().ToListAsync();
    }

    public async Task<Car?> GetByIdAsync(int id)
    {
      return await _unitOfWork.Cars.GetByIdAsync(id);
    }

    public async Task AddCarAsync(Car car)
    {
      await _unitOfWork.Cars.AddAsync(car);
      await _unitOfWork.SaveChangesAsync();
    }

    public async Task UpdateCarAsync(Car car)
    {
      _unitOfWork.Cars.Update(car);
      await _unitOfWork.SaveChangesAsync();
    }

    public async Task<bool> DeleteCarAsync(int id)
    {
      var car = await _unitOfWork.Cars.GetByIdAsync(id);
      if(car==null) return false;

      _unitOfWork.Cars.Delete(car);
      await _unitOfWork.SaveChangesAsync();
      return true;
    }
  }
}
