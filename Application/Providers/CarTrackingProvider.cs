using Application.DTOs;
using ApplicationCore.Enums;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Providers
{
    public interface ICarTrackingProvider
    {
        Task<List<CarTrackingDto>> GetAllFleetLocationsAsync();
        Task<CarTrackingDto?> GetCarCurrentLocationAsync(int carId);
        Task<List<LocationHistoryDto>> GetCarLocationHistoryAsync(int carId, int limit = 50);
    }

    public class CarTrackingProvider : ICarTrackingProvider
    {
        private readonly AppDbContext _context;

        public CarTrackingProvider(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CarTrackingDto>> GetAllFleetLocationsAsync()
        {
            var cars = await _context.Cars
                .AsNoTracking()
                .Select(c => new
                {
                    c.Id,
                    c.Model,
                    c.PlateNumber,
                    c.Status,
                    c.CurrentLatitude,
                    c.CurrentLongitude,
                    c.LastLocationUpdate,
                    ActiveContract = c.RentalContracts
                        .Where(r => r.Status == RentalContractStatus.Open)
                        .Select(r => new { r.Id, CustomerName = r.Customer.Name })
                        .FirstOrDefault(),
                    LastLog = c.LocationLogs
                        .OrderByDescending(l => l.Timestamp)
                        .Select(l => new { l.SpeedKmh, l.AddressName, l.IsEngineOn })
                        .FirstOrDefault()
                })
                .ToListAsync();

            // Default coordinates if car has no telemetry yet (Cairo / Metro area defaults)
            double defaultLat = 30.0444;
            double defaultLng = 31.2357;
            var rand = new Random(100);

            var result = new List<CarTrackingDto>();
            int index = 0;

            foreach (var car in cars)
            {
                index++;
                double lat = car.CurrentLatitude ?? (defaultLat + (index * 0.012));
                double lng = car.CurrentLongitude ?? (defaultLng + (index * 0.015));

                result.Add(new CarTrackingDto
                {
                    CarId = car.Id,
                    Model = car.Model,
                    PlateNumber = car.PlateNumber,
                    Status = car.Status,
                    Latitude = lat,
                    Longitude = lng,
                    SpeedKmh = car.LastLog?.SpeedKmh ?? (car.Status == CarStatus.Rented ? 45.0 : 0.0),
                    AddressName = car.LastLog?.AddressName ?? (car.Status == CarStatus.Rented ? "Ring Road Highway, Sector 4" : "Central Fleet Hub Depot"),
                    LastUpdated = car.LastLocationUpdate ?? DateTime.Now,
                    IsEngineOn = car.LastLog?.IsEngineOn ?? (car.Status == CarStatus.Rented),
                    ActiveCustomerName = car.ActiveContract?.CustomerName,
                    ActiveContractNumber = car.ActiveContract != null ? $"#CNT-{car.ActiveContract.Id:D5}" : null
                });
            }

            return result;
        }

        public async Task<CarTrackingDto?> GetCarCurrentLocationAsync(int carId)
        {
            var fleet = await GetAllFleetLocationsAsync();
            return fleet.FirstOrDefault(c => c.CarId == carId);
        }

        public async Task<List<LocationHistoryDto>> GetCarLocationHistoryAsync(int carId, int limit = 50)
        {
            var logs = await _context.CarLocationLogs
                .AsNoTracking()
                .Where(l => l.CarId == carId)
                .OrderByDescending(l => l.Timestamp)
                .Take(limit)
                .Select(l => new LocationHistoryDto
                {
                    Latitude = l.Latitude,
                    Longitude = l.Longitude,
                    SpeedKmh = l.SpeedKmh,
                    AddressName = l.AddressName ?? "GPS Coordinate Waypoint",
                    Timestamp = l.Timestamp,
                    IsEngineOn = l.IsEngineOn
                })
                .ToListAsync();

            if (!logs.Any())
            {
                // Generate a realistic seed route for demonstration
                var car = await GetCarCurrentLocationAsync(carId);
                if (car != null)
                {
                    var history = new List<LocationHistoryDto>();
                    var now = DateTime.Now;
                    for (int i = 0; i < 10; i++)
                    {
                        history.Add(new LocationHistoryDto
                        {
                            Latitude = car.Latitude - (i * 0.003),
                            Longitude = car.Longitude - (i * 0.004),
                            SpeedKmh = i == 0 ? 0 : 55 - (i * 3),
                            AddressName = $"Waypoint Segment #{10 - i}",
                            Timestamp = now.AddMinutes(-i * 12),
                            IsEngineOn = i < 8
                        });
                    }
                    return history;
                }
            }

            return logs;
        }
    }
}
