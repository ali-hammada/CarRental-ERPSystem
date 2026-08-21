using ApplicationCore.Entities;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
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

    public class CarServices : ICarServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public CarServices(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task<IEnumerable<Car>> GetAvailableCarsAsync()
        {
            return await _unitOfWork.Cars.GetAll()
                .Include(c => c.Category)
                .Where(c => c.Status == CarStatus.Available)
                .ToListAsync();
        }

        public async Task<IEnumerable<Car>> GetAllCarsAsync()
        {
            return await _unitOfWork.Cars.GetAll()
                .Include(c => c.Category)
                .ToListAsync();
        }

        public async Task<Car?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Cars.GetAll()
                .Include(c => c.Category)
                .Include(c => c.MaintenanceLogs)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddCarAsync(Car car)
        {
            car.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.Cars.AddAsync(car);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                null,
                "Staff User",
                "Add Vehicle",
                "Fleet",
                $"Added new vehicle '{car.Model}' ({car.PlateNumber}) at {car.PricePerDay:C}/day."
            );
        }

        public async Task UpdateCarAsync(Car car)
        {
            var existing = await _unitOfWork.Cars.GetByIdAsync(car.Id);
            if (existing == null) throw new Exception("Car record not found.");

            existing.PlateNumber = car.PlateNumber;
            existing.Model = car.Model;
            existing.Year = car.Year;
            existing.PricePerDay = car.PricePerDay;
            existing.Status = car.Status;
            existing.ImageUrl = car.ImageUrl;
            existing.CategoryId = car.CategoryId;
            existing.CurrentOdometer = car.CurrentOdometer;
            existing.FuelType = car.FuelType;
            existing.Transmission = car.Transmission;
            existing.Color = car.Color;
            existing.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.Cars.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                null,
                "Staff User",
                "Update Vehicle",
                "Fleet",
                $"Updated vehicle '{car.Model}' ({car.PlateNumber}) status to {car.Status}."
            );
        }

        public async Task<bool> DeleteCarAsync(int id)
        {
            var car = await _unitOfWork.Cars.GetByIdAsync(id);
            if (car == null) return false;

            car.IsDeleted = true;
            car.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.Cars.Update(car);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                null,
                "Staff User",
                "Delete Vehicle",
                "Fleet",
                $"Soft-deleted vehicle '{car.Model}' ({car.PlateNumber})."
            );
            return true;
        }
    }
}
