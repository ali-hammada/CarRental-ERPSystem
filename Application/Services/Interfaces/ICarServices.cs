using ApplicationCore.Entities;

namespace Application.Services.Interfaces
{
  public interface ICarServices
  {
    Task<IEnumerable<Car>> GetAvailableCarsAsync();
    Task<IEnumerable<Car>> GetAllCarsAsync();
    Task<Car?> GetByIdAsync(int id);
    Task AddCarAsync(Car car);
    Task UpdateCarAsync(Car car);
    Task<bool> DeleteCarAsync(int id);
  }
}
