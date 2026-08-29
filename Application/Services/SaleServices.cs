using Application.DTOs;
using ApplicationCore.Entities;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public interface ISaleServices
    {
        Task<(bool Success, string Message, int ContractId)> CreateSaleContractAsync(CarSaleRequestDto request, int employeeId);
        Task<(bool Success, string Message)> PayInstallmentAsync(int installmentId, string? transactionRef = null);
        Task<(bool Success, string Message)> CancelSaleContractAsync(int contractId, string? reason = null);
    }

    public class SaleServices : ISaleServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public SaleServices(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task<(bool Success, string Message, int ContractId)> CreateSaleContractAsync(CarSaleRequestDto request, int employeeId)
        {
            var car = await _unitOfWork.Cars.GetByIdAsync(request.CarId);
            if (car == null)
                return (false, "Vehicle not found.", 0);

            if (car.Status == CarStatus.Rented)
                return (false, "Cannot sell a vehicle currently out on lease/rent.", 0);

            if (car.SaleStatus == CarSaleStatus.Sold)
                return (false, "This vehicle has already been sold.", 0);

            var customer = await _unitOfWork.Customer.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return (false, "Customer record not found.", 0);

            var employee = await _unitOfWork.Employee.GetByIdAsync(employeeId);
            if (employee == null)
            {
                var fallbackEmp = (await _unitOfWork.Employee.GetAllAsync()).FirstOrDefault(e => e.IsActive);
                if (fallbackEmp == null)
                    return (false, "No active employee account found in database.", 0);
                employee = fallbackEmp;
                employeeId = fallbackEmp.Id;
            }
            string employeeName = employee.FullName;

            decimal taxAmount = Math.Round(request.SalePrice * (request.TaxRatePercent / 100m), 2);
            decimal finalPrice = request.SalePrice + taxAmount;
            decimal totalCostBasis = car.TotalCostBasis;
            decimal actualGrossProfit = finalPrice - totalCostBasis;
            bool isBelowFloor = car.MinimumFloorPrice.HasValue && car.MinimumFloorPrice.Value > 0 && request.SalePrice < car.MinimumFloorPrice.Value;

            var contract = new CarSaleContract
            {
                CarId = request.CarId,
                CustomerId = request.CustomerId,
                EmployeeId = employeeId,
                SaleDate = DateTime.UtcNow,
                SalePrice = request.SalePrice,
                TaxAmount = taxAmount,
                FinalPrice = finalPrice,
                PaymentType = request.PaymentType,
                TotalCostBasis = totalCostBasis,
                ActualGrossProfit = actualGrossProfit,
                IsBelowFloorPrice = isBelowFloor,
                DownPayment = request.PaymentType == SalePaymentType.Installment ? request.DownPayment : finalPrice,
                InstallmentMonths = request.PaymentType == SalePaymentType.Installment ? request.InstallmentMonths : 0,
                PaidAmount = request.PaymentType == SalePaymentType.Installment ? request.DownPayment : finalPrice,
                Notes = request.Notes
            };

            if (request.PaymentType == SalePaymentType.Cash)
            {
                contract.Status = SaleContractStatus.Completed;
                car.Status = CarStatus.OutOfService;
                car.SaleStatus = CarSaleStatus.Sold;
            }
            else
            {
                // Installment Financing Calculation
                if (request.DownPayment < 0 || request.DownPayment >= finalPrice)
                    return (false, "Down payment must be less than final price.", 0);

                if (request.InstallmentMonths <= 0)
                    return (false, "Installment duration must be at least 1 month.", 0);

                decimal remainingToFinance = finalPrice - request.DownPayment;
                decimal monthlyRate = Math.Round(remainingToFinance / request.InstallmentMonths, 2);
                contract.MonthlyInstallment = monthlyRate;
                contract.Status = SaleContractStatus.Active;

                car.Status = CarStatus.OutOfService;
                car.SaleStatus = CarSaleStatus.Sold;

                // Generate Installment Schedule
                var startDate = DateTime.UtcNow;
                for (int i = 1; i <= request.InstallmentMonths; i++)
                {
                    contract.Installments.Add(new SaleInstallment
                    {
                        InstallmentNumber = i,
                        DueDate = startDate.AddMonths(i),
                        Amount = monthlyRate,
                        PaidAmount = 0,
                        Status = InstallmentStatus.Pending
                    });
                }
            }

            _unitOfWork.Cars.Update(car);
            await _unitOfWork.SaveChangesAsync();

            await _unitOfWork.CarSaleContracts.AddAsync(contract);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                employeeId,
                employeeName,
                "Car Sale Contract Created",
                "Dealership Sales",
                $"Opened sale contract #{contract.Id} for vehicle '{car.Model}' ({car.PlateNumber}) to customer '{customer.Name}' for {finalPrice:C} ({request.PaymentType})."
            );

            return (true, "Car sale contract processed successfully.", contract.Id);
        }

        public async Task<(bool Success, string Message)> PayInstallmentAsync(int installmentId, string? transactionRef = null)
        {
            var installment = await _unitOfWork.SaleInstallments.GetByIdAsync(installmentId);
            if (installment == null)
                return (false, "Installment record not found.");

            if (installment.Status == InstallmentStatus.Paid)
                return (false, "This installment has already been paid.");

            var contract = await _unitOfWork.CarSaleContracts.GetByIdAsync(installment.SaleContractId);
            if (contract == null)
                return (false, "Associated sale contract not found.");

            installment.PaidAmount = installment.Amount;
            installment.PaidDate = DateTime.UtcNow;
            installment.Status = InstallmentStatus.Paid;
            installment.TransactionReference = transactionRef;

            contract.PaidAmount += installment.Amount;

            // Check if all installments are fully paid
            var allInstallments = await _unitOfWork.SaleInstallments.GetAll().Where(i => i.SaleContractId == contract.Id).ToListAsync();
            if (allInstallments.All(i => i.Status == InstallmentStatus.Paid || i.Id == installmentId))
            {
                contract.Status = SaleContractStatus.Completed;
                var car = await _unitOfWork.Cars.GetByIdAsync(contract.CarId);
                if (car != null)
                {
                    car.SaleStatus = CarSaleStatus.Sold;
                    _unitOfWork.Cars.Update(car);
                }
            }

            _unitOfWork.SaleInstallments.Update(installment);
            _unitOfWork.CarSaleContracts.Update(contract);
            await _unitOfWork.SaveChangesAsync();

            return (true, $"Installment #{installment.InstallmentNumber} paid successfully.");
        }

        public async Task<(bool Success, string Message)> CancelSaleContractAsync(int contractId, string? reason = null)
        {
            var contract = await _unitOfWork.CarSaleContracts.GetByIdAsync(contractId);
            if (contract == null)
                return (false, "Sale contract not found.");

            if (contract.Status == SaleContractStatus.Cancelled)
                return (false, "Contract is already cancelled.");

            contract.Status = SaleContractStatus.Cancelled;
            if (!string.IsNullOrEmpty(reason))
                contract.Notes = (contract.Notes ?? "") + $" [Cancelled: {reason}]";

            var car = await _unitOfWork.Cars.GetByIdAsync(contract.CarId);
            if (car != null)
            {
                car.Status = CarStatus.Available;
                car.SaleStatus = CarSaleStatus.ForSale;
                _unitOfWork.Cars.Update(car);
            }

            _unitOfWork.CarSaleContracts.Update(contract);
            await _unitOfWork.SaveChangesAsync();

            return (true, "Sale contract cancelled and vehicle restored for sale.");
        }
    }
}
