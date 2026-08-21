using Application.DTOs;
using ApplicationCore.Enums;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Providers
{
    public interface IRentalProvider
    {
        Task<List<RentalListDto>> GetEmployeeRentalsAsync(int employeeId);
        Task<List<RentalListDto>> GetCustomerRentalsAsync(int customerId);
        Task<List<RentalListDto>> GetAllRentalsAsync();
        Task<RentalListDto?> GetRentalDetailsByIdAsync(int rentalId);
        Task<List<RentalListDto>> GetActiveRentalsAsync(string? searchTerm = null);
    }

    public class RentalProvider : IRentalProvider
    {
        private readonly AppDbContext _context;

        public RentalProvider(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<RentalListDto>> GetEmployeeRentalsAsync(int employeeId)
        {
            return await _context.RentalContracts
                .AsNoTracking()
                .Where(r => r.EmployeeId == employeeId)
                .OrderByDescending(r => r.StartDate)
                .Select(r => new RentalListDto
                {
                    Id = r.Id,
                    CarId = r.CarId,
                    CarModel = r.Car.Model,
                    CarPlateNumber = r.Car.PlateNumber,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer.Name,
                    CustomerPhone = r.Customer.Phone,
                    EmployeeId = r.EmployeeId,
                    EmployeeName = r.Employee.FullName,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    ActualEndDate = r.ActualEndDate,
                    DailyPrice = r.DailyPrice,
                    TotalAmount = r.TotalAmount,
                    FinalAmount = r.FinalAmount,
                    ExtraFees = r.ExtraFees,
                    PaidAmount = r.PaidAmount,
                    Status = r.Status,
                    PaymentStatus = r.PaymentStatus,
                    Notes = r.Notes
                })
                .ToListAsync();
        }

        public async Task<List<RentalListDto>> GetCustomerRentalsAsync(int customerId)
        {
            return await _context.RentalContracts
                .AsNoTracking()
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.StartDate)
                .Select(r => new RentalListDto
                {
                    Id = r.Id,
                    CarId = r.CarId,
                    CarModel = r.Car.Model,
                    CarPlateNumber = r.Car.PlateNumber,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer.Name,
                    CustomerPhone = r.Customer.Phone,
                    EmployeeId = r.EmployeeId,
                    EmployeeName = r.Employee.FullName,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    ActualEndDate = r.ActualEndDate,
                    DailyPrice = r.DailyPrice,
                    TotalAmount = r.TotalAmount,
                    FinalAmount = r.FinalAmount,
                    ExtraFees = r.ExtraFees,
                    PaidAmount = r.PaidAmount,
                    Status = r.Status,
                    PaymentStatus = r.PaymentStatus,
                    Notes = r.Notes
                })
                .ToListAsync();
        }

        public async Task<List<RentalListDto>> GetAllRentalsAsync()
        {
            return await _context.RentalContracts
                .AsNoTracking()
                .OrderByDescending(r => r.StartDate)
                .Select(r => new RentalListDto
                {
                    Id = r.Id,
                    CarId = r.CarId,
                    CarModel = r.Car.Model,
                    CarPlateNumber = r.Car.PlateNumber,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer.Name,
                    CustomerPhone = r.Customer.Phone,
                    EmployeeId = r.EmployeeId,
                    EmployeeName = r.Employee.FullName,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    ActualEndDate = r.ActualEndDate,
                    DailyPrice = r.DailyPrice,
                    TotalAmount = r.TotalAmount,
                    FinalAmount = r.FinalAmount,
                    ExtraFees = r.ExtraFees,
                    PaidAmount = r.PaidAmount,
                    Status = r.Status,
                    PaymentStatus = r.PaymentStatus,
                    Notes = r.Notes
                })
                .ToListAsync();
        }

        public async Task<RentalListDto?> GetRentalDetailsByIdAsync(int rentalId)
        {
            return await _context.RentalContracts
                .AsNoTracking()
                .Where(r => r.Id == rentalId)
                .Select(r => new RentalListDto
                {
                    Id = r.Id,
                    CarId = r.CarId,
                    CarModel = r.Car.Model,
                    CarPlateNumber = r.Car.PlateNumber,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer.Name,
                    CustomerPhone = r.Customer.Phone,
                    EmployeeId = r.EmployeeId,
                    EmployeeName = r.Employee.FullName,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    ActualEndDate = r.ActualEndDate,
                    DailyPrice = r.DailyPrice,
                    TotalAmount = r.TotalAmount,
                    FinalAmount = r.FinalAmount,
                    ExtraFees = r.ExtraFees,
                    PaidAmount = r.PaidAmount,
                    Status = r.Status,
                    PaymentStatus = r.PaymentStatus,
                    Notes = r.Notes
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<RentalListDto>> GetActiveRentalsAsync(string? searchTerm = null)
        {
            var query = _context.RentalContracts
                .AsNoTracking()
                .Where(r => r.Status == RentalContractStatus.Open);

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(r =>
                    r.Car.PlateNumber.Contains(searchTerm) ||
                    r.Customer.Name.Contains(searchTerm) ||
                    r.Car.Model.Contains(searchTerm));
            }

            return await query
                .OrderByDescending(r => r.StartDate)
                .Select(r => new RentalListDto
                {
                    Id = r.Id,
                    CarId = r.CarId,
                    CarModel = r.Car.Model,
                    CarPlateNumber = r.Car.PlateNumber,
                    CustomerId = r.CustomerId,
                    CustomerName = r.Customer.Name,
                    CustomerPhone = r.Customer.Phone,
                    EmployeeId = r.EmployeeId,
                    EmployeeName = r.Employee.FullName,
                    StartDate = r.StartDate,
                    EndDate = r.EndDate,
                    ActualEndDate = r.ActualEndDate,
                    DailyPrice = r.DailyPrice,
                    TotalAmount = r.TotalAmount,
                    FinalAmount = r.FinalAmount,
                    ExtraFees = r.ExtraFees,
                    PaidAmount = r.PaidAmount,
                    Status = r.Status,
                    PaymentStatus = r.PaymentStatus,
                    Notes = r.Notes
                })
                .ToListAsync();
        }
    }
}
