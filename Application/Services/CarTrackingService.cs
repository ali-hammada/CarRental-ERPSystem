using ApplicationCore.Entities;
using ApplicationCore.Enums;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public interface ICarTrackingService
    {
        Task<bool> UpdateTelemetryAsync(int carId, double lat, double lng, double speedKmh, bool isEngineOn, string? addressName = null);
        Task<bool> SimulateTelemetryPingAsync(int carId);
    }

    public class CarTrackingService : ICarTrackingService
    {
        private readonly AppDbContext _context;

        public CarTrackingService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<bool> UpdateTelemetryAsync(int carId, double lat, double lng, double speedKmh, bool isEngineOn, string? addressName = null)
        {
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == carId);
            if (car == null) return false;

            car.CurrentLatitude = lat;
            car.CurrentLongitude = lng;
            car.LastLocationUpdate = DateTime.UtcNow;

            var log = new CarLocationLog
            {
                CarId = carId,
                Latitude = lat,
                Longitude = lng,
                SpeedKmh = speedKmh,
                IsEngineOn = isEngineOn,
                AddressName = addressName ?? $"Lat: {lat:F4}, Lng: {lng:F4}",
                Timestamp = DateTime.UtcNow
            };

            _context.CarLocationLogs.Add(log);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> SimulateTelemetryPingAsync(int carId)
        {
            var car = await _context.Cars.FirstOrDefaultAsync(c => c.Id == carId);
            if (car == null) return false;

            var rand = new Random();
            double curLat = car.CurrentLatitude ?? 30.0444;
            double curLng = car.CurrentLongitude ?? 31.2357;

            // Simulate realistic GPS shift (+/- 0.005 degrees)
            double nextLat = curLat + ((rand.NextDouble() - 0.48) * 0.008);
            double nextLng = curLng + ((rand.NextDouble() - 0.48) * 0.008);
            double speed = car.Status == CarStatus.Rented ? rand.Next(30, 95) : 0;
            bool engineOn = car.Status == CarStatus.Rented || rand.Next(0, 2) == 1;

            return await UpdateTelemetryAsync(carId, nextLat, nextLng, speed, engineOn, "Live GPS Telemetry Waypoint");
        }
    }
}
