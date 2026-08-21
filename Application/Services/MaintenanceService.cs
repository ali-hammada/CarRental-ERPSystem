using ApplicationCore.Entities;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public interface IMaintenanceService
    {
        Task<IEnumerable<MaintenanceLog>> GetAllLogsAsync();
        Task<IEnumerable<MaintenanceLog>> GetLogsByCarIdAsync(int carId);
        Task<MaintenanceLog?> GetByIdAsync(int id);
        Task AddLogAsync(MaintenanceLog log);
        Task UpdateLogAsync(MaintenanceLog log);
        Task<bool> DeleteLogAsync(int id);
    }

    public class MaintenanceService : IMaintenanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public MaintenanceService(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task<IEnumerable<MaintenanceLog>> GetAllLogsAsync()
        {
            return await _unitOfWork.MaintenanceLogs.GetAll()
                .Include(m => m.Car)
                .OrderByDescending(m => m.ServiceDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<MaintenanceLog>> GetLogsByCarIdAsync(int carId)
        {
            return await _unitOfWork.MaintenanceLogs.GetAll()
                .Include(m => m.Car)
                .Where(m => m.CarId == carId)
                .OrderByDescending(m => m.ServiceDate)
                .ToListAsync();
        }

        public async Task<MaintenanceLog?> GetByIdAsync(int id)
        {
            return await _unitOfWork.MaintenanceLogs.GetAll()
                .Include(m => m.Car)
                .FirstOrDefaultAsync(m => m.Id == id);
        }

        public async Task AddLogAsync(MaintenanceLog log)
        {
            log.CreatedAt = DateTime.UtcNow;
            await _unitOfWork.MaintenanceLogs.AddAsync(log);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                null,
                "Staff User",
                "Log Maintenance",
                "Maintenance",
                $"Logged '{log.ServiceType}' service for Vehicle #{log.CarId} (Cost: {log.Cost:C}, Provider: {log.PerformedBy})."
            );
        }

        public async Task UpdateLogAsync(MaintenanceLog log)
        {
            var existing = await _unitOfWork.MaintenanceLogs.GetByIdAsync(log.Id);
            if (existing == null) throw new Exception("Maintenance log entry not found.");

            existing.ServiceType = log.ServiceType;
            existing.Description = log.Description;
            existing.Cost = log.Cost;
            existing.ServiceDate = log.ServiceDate;
            existing.OdometerReading = log.OdometerReading;
            existing.PerformedBy = log.PerformedBy;
            existing.Status = log.Status;
            existing.UpdatedAt = DateTime.UtcNow;

            _unitOfWork.MaintenanceLogs.Update(existing);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                null,
                "Staff User",
                "Update Maintenance",
                "Maintenance",
                $"Updated maintenance log #{log.Id} ('{log.ServiceType}')."
            );
        }

        public async Task<bool> DeleteLogAsync(int id)
        {
            var log = await _unitOfWork.MaintenanceLogs.GetByIdAsync(id);
            if (log == null) return false;

            log.IsDeleted = true;
            log.UpdatedAt = DateTime.UtcNow;
            _unitOfWork.MaintenanceLogs.Update(log);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                null,
                "Staff User",
                "Delete Maintenance",
                "Maintenance",
                $"Soft-deleted maintenance log #{id}."
            );
            return true;
        }
    }
}
