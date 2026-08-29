using Application.DTOs;
using ApplicationCore.Enums;
using InFrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Application.Providers
{
    public interface ISaleProvider
    {
        Task<List<CarSaleListDto>> GetAllSaleContractsAsync();
        Task<CarSaleListDto?> GetSaleContractDetailsByIdAsync(int id);
        Task<List<CarListDto>> GetCarsAvailableForSaleAsync(int? includeCarId = null);
        Task<List<SaleInstallmentDto>> GetContractInstallmentsAsync(int saleContractId);
        Task<List<SaleInstallmentDto>> GetOverdueInstallmentsAsync();
        Task<CarSaleNegotiationMetadataDto?> GetNegotiationMetadataAsync(int carId);
    }

    public class SaleProvider : ISaleProvider
    {
        private readonly AppDbContext _context;

        public SaleProvider(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<CarSaleListDto>> GetAllSaleContractsAsync()
        {
            return await _context.CarSaleContracts
                .AsNoTracking()
                .OrderByDescending(s => s.SaleDate)
                .Select(s => new CarSaleListDto
                {
                    Id = s.Id,
                    CarId = s.CarId,
                    CarModel = s.Car.Model,
                    CarPlateNumber = s.Car.PlateNumber,
                    CustomerId = s.CustomerId,
                    CustomerName = s.Customer.Name,
                    CustomerPhone = s.Customer.Phone,
                    EmployeeId = s.EmployeeId,
                    EmployeeName = s.Employee.FullName,
                    SaleDate = s.SaleDate,
                    SalePrice = s.SalePrice,
                    TaxAmount = s.TaxAmount,
                    FinalPrice = s.FinalPrice,
                    PaymentType = s.PaymentType,
                    DownPayment = s.DownPayment,
                    InstallmentMonths = s.InstallmentMonths,
                    MonthlyInstallment = s.MonthlyInstallment,
                    PaidAmount = s.PaidAmount,
                    TotalCostBasis = s.TotalCostBasis,
                    ActualGrossProfit = s.ActualGrossProfit,
                    IsBelowFloorPrice = s.IsBelowFloorPrice,
                    Status = s.Status,
                    Notes = s.Notes
                })
                .ToListAsync();
        }

        public async Task<CarSaleListDto?> GetSaleContractDetailsByIdAsync(int id)
        {
            return await _context.CarSaleContracts
                .AsNoTracking()
                .Where(s => s.Id == id)
                .Select(s => new CarSaleListDto
                {
                    Id = s.Id,
                    CarId = s.CarId,
                    CarModel = s.Car.Model,
                    CarPlateNumber = s.Car.PlateNumber,
                    CustomerId = s.CustomerId,
                    CustomerName = s.Customer.Name,
                    CustomerPhone = s.Customer.Phone,
                    EmployeeId = s.EmployeeId,
                    EmployeeName = s.Employee.FullName,
                    SaleDate = s.SaleDate,
                    SalePrice = s.SalePrice,
                    TaxAmount = s.TaxAmount,
                    FinalPrice = s.FinalPrice,
                    PaymentType = s.PaymentType,
                    DownPayment = s.DownPayment,
                    InstallmentMonths = s.InstallmentMonths,
                    MonthlyInstallment = s.MonthlyInstallment,
                    PaidAmount = s.PaidAmount,
                    TotalCostBasis = s.TotalCostBasis,
                    ActualGrossProfit = s.ActualGrossProfit,
                    IsBelowFloorPrice = s.IsBelowFloorPrice,
                    Status = s.Status,
                    Notes = s.Notes
                })
                .FirstOrDefaultAsync();
        }

        public async Task<CarSaleNegotiationMetadataDto?> GetNegotiationMetadataAsync(int carId)
        {
            return await _context.Cars
                .AsNoTracking()
                .Where(c => c.Id == carId)
                .Select(c => new CarSaleNegotiationMetadataDto
                {
                    CarId = c.Id,
                    CarModel = c.Model,
                    CarPlateNumber = c.PlateNumber,
                    PurchasePrice = c.PurchasePrice ?? 0m,
                    RefurbishmentCost = c.RefurbishmentCost ?? 0m,
                    TotalCostBasis = (c.PurchasePrice ?? 0m) + (c.RefurbishmentCost ?? 0m),
                    TargetSalePrice = c.TargetSalePrice ?? c.SalePrice ?? 0m,
                    MinimumFloorPrice = c.MinimumFloorPrice ?? 0m
                })
                .FirstOrDefaultAsync();
        }

        public async Task<List<CarListDto>> GetCarsAvailableForSaleAsync(int? includeCarId = null)
        {
            return await _context.Cars
                .AsNoTracking()
                .Where(c => (c.SaleStatus == null || c.SaleStatus != CarSaleStatus.Sold) &&
                            (c.Status != CarStatus.Rented || (includeCarId.HasValue && c.Id == includeCarId.Value)))
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
                    Color = c.Color,
                    LicenseExpiryDate = c.LicenseExpiryDate,
                    InsuranceExpiryDate = c.InsuranceExpiryDate,
                    ListingType = c.ListingType,
                    SalePrice = c.SalePrice,
                    SaleStatus = c.SaleStatus,
                    PurchasePrice = c.PurchasePrice,
                    PurchaseDate = c.PurchaseDate,
                    RefurbishmentCost = c.RefurbishmentCost,
                    TargetSalePrice = c.TargetSalePrice,
                    MinimumFloorPrice = c.MinimumFloorPrice,
                    OriginalPurchaseContractUrl = c.OriginalPurchaseContractUrl,
                    FinalBuyerContractUrl = c.FinalBuyerContractUrl
                })
                .ToListAsync();
        }

        public async Task<List<SaleInstallmentDto>> GetContractInstallmentsAsync(int saleContractId)
        {
            return await _context.SaleInstallments
                .AsNoTracking()
                .Where(i => i.SaleContractId == saleContractId)
                .OrderBy(i => i.InstallmentNumber)
                .Select(i => new SaleInstallmentDto
                {
                    Id = i.Id,
                    SaleContractId = i.SaleContractId,
                    InstallmentNumber = i.InstallmentNumber,
                    DueDate = i.DueDate,
                    Amount = i.Amount,
                    PaidAmount = i.PaidAmount,
                    PaidDate = i.PaidDate,
                    Status = i.Status,
                    TransactionReference = i.TransactionReference
                })
                .ToListAsync();
        }

        public async Task<List<SaleInstallmentDto>> GetOverdueInstallmentsAsync()
        {
            var now = DateTime.UtcNow;
            return await _context.SaleInstallments
                .AsNoTracking()
                .Where(i => i.Status == InstallmentStatus.Pending && i.DueDate < now)
                .OrderBy(i => i.DueDate)
                .Select(i => new SaleInstallmentDto
                {
                    Id = i.Id,
                    SaleContractId = i.SaleContractId,
                    InstallmentNumber = i.InstallmentNumber,
                    DueDate = i.DueDate,
                    Amount = i.Amount,
                    PaidAmount = i.PaidAmount,
                    PaidDate = i.PaidDate,
                    Status = i.Status,
                    TransactionReference = i.TransactionReference
                })
                .ToListAsync();
        }
    }
}
