using Application.Services.DTOs;
using ApplicationCore.Entities;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public interface IRentalServices
    {
        Task<RentalContract?> GetRentalByIdAsync(int rentalId);
        Task<(bool Success, string Content, int id)> OpenRequestRentalAsync(RentalRequestDTO request, int employeeId);
        Task<(bool Success, string Content, int id)> CancelRentalAsync(int rentalId, int employeeId, string? reason = null);
        Task<(bool Success, string Content, int id)> ExtendContractAsync(ExtendRentalDto extend, int employeeId);
        Task<(bool Success, string Content, int id)> CloseContractAsync(RentalCloseDto request, int employeeId);

        Task<bool> HasActiveRentalAsync(int carId, DateTime start, DateTime end);
        Task<List<RentalContract>> GetEmployeesRentalsAsync(int employeeId);
        Task<List<RentalContract>> GetEmployeesRentalsWithCarsAsync(int employeeId);
        Task<List<RentalContract>> GetCustomerRentalsAsync(int customerId);
        Task<List<RentalContract>> GetAllRentalsAsync();
    }

    public class RentalServices : IRentalServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public RentalServices(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task<(bool Success, string Content, int id)> OpenRequestRentalAsync(RentalRequestDTO request, int employeeId)
        {
            var car = await _unitOfWork.Cars.GetByIdAsync(request.CarId);
            if (car == null || car.Status != CarStatus.Available)
                return (false, "Selected vehicle is not available for rental.", 0);

            var customer = await _unitOfWork.Customer.GetByIdAsync(request.CustomerId);
            if (customer == null)
                return (false, "Selected customer profile does not exist.", 0);

            if (customer.LicenseExpiryDate.Date < request.EndDate.Date)
                return (false, $"Customer's driving license expires on {customer.LicenseExpiryDate:yyyy-MM-dd}, which is before the rental end date ({request.EndDate:yyyy-MM-dd}).", 0);

            if (request.StartDate.Date < DateTime.UtcNow.Date)
                return (false, "Contract start date cannot be in the past.", 0);

            if (request.EndDate.Date <= request.StartDate.Date)
                return (false, "Contract end date must be after the start date.", 0);

            bool hasConflict = await _unitOfWork.RentalContracts.HasActiveRentalAsync(request.CarId, request.StartDate, request.EndDate);
            if (hasConflict)
                return (false, "Vehicle is already reserved for an overlapping period.", 0);

            int numberOfDays = Math.Max(1, (request.EndDate.Date - request.StartDate.Date).Days + 1);
            decimal dailyPrice = car.PricePerDay;
            decimal totalAmount = dailyPrice * numberOfDays;
            int startOdometer = request.StartOdometer > 0 ? request.StartOdometer : car.CurrentOdometer;

            var rental = new RentalContract
            {
                CarId = request.CarId,
                CustomerId = request.CustomerId,
                EmployeeId = employeeId,
                StartDate = request.StartDate,
                EndDate = request.EndDate,
                DailyPrice = dailyPrice,
                TotalAmount = totalAmount,
                Status = RentalContractStatus.Open,
                PaymentStatus = PaymentStatus.Unpaid,
                StartOdometer = startOdometer,
                StartFuelLevel = request.StartFuelLevel ?? "Full",
                DepositAmount = request.DepositAmount,
                Notes = request.Notes ?? string.Empty,
                CreatedAt = DateTime.UtcNow
            };

            await _unitOfWork.RentalContracts.AddAsync(rental);
            car.Status = CarStatus.Rented;
            _unitOfWork.Cars.Update(car);

            await _unitOfWork.SaveChangesAsync();

            // Auto-generate Tax Invoice Draft
            var invoice = new Invoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{rental.Id:D5}",
                RentalContractId = rental.Id,
                IssueDate = DateTime.UtcNow,
                SubTotal = totalAmount,
                TaxRate = 0.14m,
                TaxAmount = Math.Round(totalAmount * 0.14m, 2),
                TotalAmount = Math.Round(totalAmount * 1.14m, 2),
                PaidAmount = 0,
                Status = "Issued"
            };

            await _unitOfWork.Invoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                employeeId,
                null,
                "Open Rental Contract",
                "Rentals",
                $"Issued new Contract #CNT-{rental.Id:D5} for '{car.Model}' to customer '{customer.Name}' ({numberOfDays} days, {totalAmount:C})."
            );

            return (true, "Rental contract opened successfully.", rental.Id);
        }

        public async Task<(bool Success, string Content, int id)> CancelRentalAsync(int rentalId, int employeeId, string? reason = null)
        {
            var contract = await _unitOfWork.RentalContracts.GetByIdAsync(rentalId);
            if (contract == null)
                return (false, "Rental contract standard record not found.", 0);

            if (contract.Status != RentalContractStatus.Open && contract.Status != RentalContractStatus.Draft)
                return (false, "Only open or draft contracts can be cancelled.", 0);

            var car = await _unitOfWork.Cars.GetByIdAsync(contract.CarId);

            decimal cancellationFee = 0;
            DateTime now = DateTime.UtcNow;

            if (now.Date < contract.StartDate.Date)
            {
                int daysUntilStart = (contract.StartDate.Date - now.Date).Days;
                if (daysUntilStart < 1) cancellationFee = contract.TotalAmount * 0.5m;
                else if (daysUntilStart < 3) cancellationFee = contract.TotalAmount * 0.25m;
                else if (daysUntilStart < 7) cancellationFee = contract.TotalAmount * 0.15m;
            }
            else
            {
                int usedDays = Math.Max(1, (now.Date - contract.StartDate.Date).Days + 1);
                decimal usedAmount = usedDays * contract.DailyPrice;
                decimal remainingAmount = Math.Max(0, contract.TotalAmount - usedAmount);
                cancellationFee = usedAmount + (remainingAmount * 0.2m);
            }

            contract.Status = RentalContractStatus.Cancelled;
            contract.ActualEndDate = now;
            contract.ExtraFees = cancellationFee;
            contract.FinalAmount = cancellationFee;
            contract.UpdatedAt = now;

            if (!string.IsNullOrWhiteSpace(reason))
                contract.Notes += $"\n[Cancelled: {now:yyyy-MM-dd}] Reason: {reason}";

            if (car != null)
            {
                car.Status = CarStatus.Available;
                _unitOfWork.Cars.Update(car);
            }

            _unitOfWork.RentalContracts.Update(contract);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                employeeId,
                null,
                "Cancel Contract",
                "Rentals",
                $"Cancelled Contract #CNT-{rentalId:D5}. Cancellation Fee: {cancellationFee:C}. Reason: {reason}."
            );

            return (true, $"Contract cancelled. Final Cancellation Fee: {cancellationFee:C}", rentalId);
        }

        public async Task<(bool Success, string Content, int id)> ExtendContractAsync(ExtendRentalDto extend, int employeeId)
        {
            var contract = await _unitOfWork.RentalContracts.GetByIdAsync(extend.RentalId);
            if (contract == null)
                return (false, "Rental contract record not found.", 0);

            if (contract.Status != RentalContractStatus.Open)
                return (false, "Only active open contracts can be extended.", 0);

            if (!extend.NewEndDate.HasValue || extend.NewEndDate.Value.Date <= contract.EndDate.Date)
                return (false, "New end date must be strictly after the current end date.", 0);

            var customer = await _unitOfWork.Customer.GetByIdAsync(contract.CustomerId);
            if (customer != null && customer.LicenseExpiryDate.Date < extend.NewEndDate.Value.Date)
                return (false, $"Customer's license expires on {customer.LicenseExpiryDate:yyyy-MM-dd}, before the requested extension date.", 0);

            bool hasConflict = await _unitOfWork.RentalContracts.HasActiveRentalAsync(
                contract.CarId,
                contract.EndDate.AddDays(1),
                extend.NewEndDate.Value);

            if (hasConflict)
                return (false, "Vehicle is reserved by another client during the extension period.", 0);

            int extraDays = (extend.NewEndDate.Value.Date - contract.EndDate.Date).Days;
            decimal extraAmount = extraDays * contract.DailyPrice;

            contract.EndDate = extend.NewEndDate.Value;
            contract.TotalAmount += extraAmount;
            contract.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(extend.Notes))
                contract.Notes += $"\n[Extended by {extraDays} days on {DateTime.UtcNow:yyyy-MM-dd}] {extend.Notes}";

            _unitOfWork.RentalContracts.Update(contract);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                employeeId,
                null,
                "Extend Contract",
                "Rentals",
                $"Extended Contract #CNT-{contract.Id:D5} by {extraDays} days to {extend.NewEndDate.Value:yyyy-MM-dd} (Extra: {extraAmount:C})."
            );

            return (true, $"Contract extended by {extraDays} days. Additional charges: {extraAmount:C}", contract.Id);
        }

        public async Task<(bool Success, string Content, int id)> CloseContractAsync(RentalCloseDto request, int employeeId)
        {
            var rental = await _unitOfWork.RentalContracts.GetByIdAsync(request.RentalId);
            if (rental == null)
                return (false, "Rental contract not found.", 0);

            if (rental.Status != RentalContractStatus.Open)
                return (false, "Only open active contracts can be closed.", 0);

            var car = await _unitOfWork.Cars.GetByIdAsync(rental.CarId);

            rental.ActualEndDate = DateTime.UtcNow;
            rental.EndOdometer = request.EndOdometer > 0 ? request.EndOdometer : (car?.CurrentOdometer ?? rental.StartOdometer);
            rental.EndFuelLevel = request.EndFuelLevel ?? "Full";

            int actualDays = Math.Max(1, (rental.ActualEndDate.Value.Date - rental.StartDate.Date).Days + 1);
            int expectedDays = Math.Max(1, (rental.EndDate.Date - rental.StartDate.Date).Days + 1);

            decimal baseAmount = actualDays * rental.DailyPrice;
            decimal latePenalty = 0;

            if (actualDays > expectedDays)
            {
                int lateDays = actualDays - expectedDays;
                latePenalty = lateDays * rental.DailyPrice * 1.5m;
            }

            rental.ExtraFees = latePenalty + request.ExtraFees;
            rental.FinalAmount = baseAmount + rental.ExtraFees.Value;
            rental.Status = RentalContractStatus.Closed;
            rental.UpdatedAt = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(request.DamageNotes))
                rental.Notes += $"\n[Return Inspection Notes]: {request.DamageNotes}";

            if (car != null)
            {
                if (request.EndOdometer > car.CurrentOdometer)
                    car.CurrentOdometer = request.EndOdometer;

                car.Status = CarStatus.Available;
                _unitOfWork.Cars.Update(car);
            }

            _unitOfWork.RentalContracts.Update(rental);
            await _unitOfWork.SaveChangesAsync();

            await _auditLogService.LogActionAsync(
                employeeId,
                null,
                "Close & Return Contract",
                "Rentals",
                $"Returned vehicle & closed Contract #CNT-{rental.Id:D5}. Final amount settled: {rental.FinalAmount:C}."
            );

            return (true, $"Contract closed successfully. Final total: {rental.FinalAmount:C}", rental.Id);
        }

        public async Task<bool> HasActiveRentalAsync(int carId, DateTime start, DateTime end)
        {
            return await _unitOfWork.RentalContracts.HasActiveRentalAsync(carId, start, end);
        }

        public async Task<RentalContract?> GetRentalByIdAsync(int rentalId)
        {
            return await _unitOfWork.RentalContracts.GetAll()
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .Include(r => r.Employee)
                .Include(r => r.Payments)
                .Include(r => r.Invoices)
                .FirstOrDefaultAsync(r => r.Id == rentalId);
        }

        public async Task<List<RentalContract>> GetEmployeesRentalsAsync(int employeeId)
        {
            return await _unitOfWork.RentalContracts.GetAll()
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .Where(r => r.EmployeeId == employeeId)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
        }

        public async Task<List<RentalContract>> GetEmployeesRentalsWithCarsAsync(int employeeId)
        {
            return await _unitOfWork.RentalContracts.GetAll()
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .Include(r => r.Employee)
                .Where(r => r.EmployeeId == employeeId)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
        }

        public async Task<List<RentalContract>> GetCustomerRentalsAsync(int customerId)
        {
            return await _unitOfWork.RentalContracts.GetAll()
                .Include(r => r.Car)
                .Include(r => r.Employee)
                .Where(r => r.CustomerId == customerId)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
        }

        public async Task<List<RentalContract>> GetAllRentalsAsync()
        {
            return await _unitOfWork.RentalContracts.GetAll()
                .Include(r => r.Car)
                .Include(r => r.Customer)
                .Include(r => r.Employee)
                .OrderByDescending(r => r.StartDate)
                .ToListAsync();
        }
    }
}
