using Application.DTOs;
using ApplicationCore.Enums;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Providers
{
    public interface ICarProvider
    {
        Task<List<CarListDto>> GetAllCarsAsync();
        Task<List<CarListDto>> GetAvailableCarsAsync();
        Task<CarListDto?> GetCarByIdAsync(int id);
    }

    public class CarProvider : ICarProvider
    {
        private readonly AppDbContext _context;

        public CarProvider(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CarListDto>> GetAllCarsAsync()
        {
            return await _context.Cars
                .AsNoTracking()
                .Select(c => new CarListDto
                {
                    Id = c.Id,
                    PlateNumber = c.PlateNumber,
                    Model = c.Model,
                    Year = c.Year,
                    PricePerDay = c.PricePerDay,
                    Status = c.Status,
                    ImageUrl = c.ImageUrl,
                    CategoryName = c.Category != null ? c.Category.Name : "Standard",
                    CurrentOdometer = c.CurrentOdometer,
                    FuelType = c.FuelType,
                    Transmission = c.Transmission,
                    Color = c.Color
                })
                .ToListAsync();
        }

        public async Task<List<CarListDto>> GetAvailableCarsAsync()
        {
            return await _context.Cars
                .AsNoTracking()
                .Where(c => c.Status == CarStatus.Available)
                .Select(c => new CarListDto
                {
                    Id = c.Id,
                    PlateNumber = c.PlateNumber,
                    Model = c.Model,
                    Year = c.Year,
                    PricePerDay = c.PricePerDay,
                    Status = c.Status,
                    ImageUrl = c.ImageUrl,
                    CategoryName = c.Category != null ? c.Category.Name : "Standard",
                    CurrentOdometer = c.CurrentOdometer,
                    FuelType = c.FuelType,
                    Transmission = c.Transmission,
                    Color = c.Color
                })
                .ToListAsync();
        }

        public async Task<CarListDto?> GetCarByIdAsync(int id)
        {
            return await _context.Cars
                .AsNoTracking()
                .Where(c => c.Id == id)
                .Select(c => new CarListDto
                {
                    Id = c.Id,
                    PlateNumber = c.PlateNumber,
                    Model = c.Model,
                    Year = c.Year,
                    PricePerDay = c.PricePerDay,
                    Status = c.Status,
                    ImageUrl = c.ImageUrl,
                    CategoryName = c.Category != null ? c.Category.Name : "Standard",
                    CurrentOdometer = c.CurrentOdometer,
                    FuelType = c.FuelType,
                    Transmission = c.Transmission,
                    Color = c.Color
                })
                .FirstOrDefaultAsync();
        }
    }
}
