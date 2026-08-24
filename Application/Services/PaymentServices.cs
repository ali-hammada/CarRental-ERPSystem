using ApplicationCore.Entities;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public interface IPaymentServices
    {
        Task<(bool success, string message, int PaymentId)> MakePaymentAsync(int rentalContractId, decimal amount, PaymentPurpose purpose, PaymentMethod method, int employeeId, string? transactionRef = null);
        Task<(bool success, string message, decimal remaining)> GetRemainingAmountAsync(int rentalContractId, int employeeId);
        Task<List<Payment>> GetContractPaymentsAsync(int rentalContractId, int employeeId);
        Task<Payment?> GetPaymentByIdAsync(int paymentId, int employeeId);
        Task<List<Payment>> GetAllEmployeesPaymentsAsync(int employeeId);
        Task<List<Payment>> GetAllPaymentsAsync();
    }

    public class PaymentServices : IPaymentServices
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuditLogService _auditLogService;

        public PaymentServices(IUnitOfWork unitOfWork, IAuditLogService auditLogService)
        {
            _unitOfWork = unitOfWork;
            _auditLogService = auditLogService;
        }

        public async Task<(bool success, string message, int PaymentId)> MakePaymentAsync(
            int rentalContractId,
            decimal amount,
            PaymentPurpose purpose,
            PaymentMethod method,
            int employeeId,
            string? transactionRef = null)
        {
            var contract = await _unitOfWork.RentalContracts.GetByIdAsync(rentalContractId);
            if (contract == null)
                return (false, "Rental contract standard record not found.", 0);

            if (amount <= 0)
                return (false, "Payment amount must be greater than zero.", 0);

            if (contract.Status == RentalContractStatus.Cancelled)
                return (false, "Cannot process payments for a cancelled contract.", 0);

            decimal amountDue = contract.FinalAmount ?? contract.TotalAmount;
            decimal remainingBefore = amountDue - contract.PaidAmount;

            Payment payment;
            string message;

            switch (purpose)
            {
                case PaymentPurpose.Partial:
                    if (contract.PaidAmount + amount > amountDue)
                        return (false, $"Payment exceeds total remaining amount of {remainingBefore:C}", 0);

                    contract.PaidAmount += amount;
                    contract.PaymentStatus = contract.PaidAmount >= amountDue 
                        ? PaymentStatus.Paid 
                        : PaymentStatus.PartiallyPaid;

                    payment = new Payment
                    {
                        RentalContractId = rentalContractId,
                        Amount = amount,
                        Purpose = purpose,
                        Method = method,
                        Status = PaymentStatus.Paid,
                        PaymentDate = DateTime.UtcNow,
                        TransactionReference = transactionRef
                    };

                    await _unitOfWork.Payments.AddAsync(payment);
                    _unitOfWork.RentalContracts.Update(contract);
                    await _unitOfWork.SaveChangesAsync();

                    await _auditLogService.LogActionAsync(
                        employeeId,
                        null,
                        "Process Partial Payment",
                        "Payments",
                        $"Processed payment of {amount:C} via {method} for Contract #CNT-{rentalContractId:D5}. Remaining: {(amountDue - contract.PaidAmount):C}."
                    );

                    decimal remainingAfter = amountDue - contract.PaidAmount;
                    message = $"Partial payment of {amount:C} registered successfully. Remaining balance: {remainingAfter:C}";
                    return (true, message, payment.Id);

                case PaymentPurpose.Final:
                    if (amount < remainingBefore)
                        return (false, $"Final payment amount must be at least {remainingBefore:C}", 0);

                    decimal overpayment = amount - remainingBefore;
                    contract.PaidAmount = amountDue;
                    contract.PaymentStatus = PaymentStatus.Paid;

                    payment = new Payment
                    {
                        RentalContractId = rentalContractId,
                        Amount = amount,
                        Purpose = purpose,
                        Method = method,
                        Status = PaymentStatus.Paid,
                        PaymentDate = DateTime.UtcNow,
                        TransactionReference = transactionRef
                    };

                    await _unitOfWork.Payments.AddAsync(payment);
                    _unitOfWork.RentalContracts.Update(contract);
                    await _unitOfWork.SaveChangesAsync();

                    await _auditLogService.LogActionAsync(
                        employeeId,
                        null,
                        "Process Final Settlement",
                        "Payments",
                        $"Settled Contract #CNT-{rentalContractId:D5} in full with {amount:C} via {method}."
                    );

                    message = overpayment > 0
                        ? $"Final settlement completed. Overpayment change: {overpayment:C}"
                        : $"Final payment of {amount:C} completed successfully.";

                    return (true, message, payment.Id);

                case PaymentPurpose.Penalty:
                    contract.ExtraFees = (contract.ExtraFees ?? 0) + amount;
                    contract.FinalAmount = (contract.FinalAmount ?? contract.TotalAmount) + amount;
                    contract.PaidAmount += amount;

                    payment = new Payment
                    {
                        RentalContractId = rentalContractId,
                        Amount = amount,
                        Purpose = purpose,
                        Method = method,
                        Status = PaymentStatus.Paid,
                        PaymentDate = DateTime.UtcNow,
                        TransactionReference = transactionRef
                    };

                    await _unitOfWork.Payments.AddAsync(payment);
                    _unitOfWork.RentalContracts.Update(contract);
                    await _unitOfWork.SaveChangesAsync();

                    await _auditLogService.LogActionAsync(
                        employeeId,
                        null,
                        "Process Penalty Payment",
                        "Payments",
                        $"Processed penalty/extra fee payment of {amount:C} for Contract #CNT-{rentalContractId:D5}."
                    );

                    message = $"Penalty payment of {amount:C} logged and settled. New final amount: {contract.FinalAmount:C}";
                    return (true, message, payment.Id);

                default:
                    return (false, "Invalid payment purpose specified.", 0);
            }
        }

        public async Task<(bool success, string message, decimal remaining)> GetRemainingAmountAsync(int rentalContractId, int employeeId)
        {
            var contract = await _unitOfWork.RentalContracts.GetByIdAsync(rentalContractId);
            if (contract == null)
                return (false, "Rental contract record not found.", 0);

            decimal amountDue = contract.FinalAmount ?? contract.TotalAmount;
            decimal remaining = Math.Max(0, amountDue - contract.PaidAmount);

            return (true, $"Remaining balance: {remaining:C}", remaining);
        }

        public async Task<List<Payment>> GetContractPaymentsAsync(int rentalContractId, int employeeId)
        {
            return await _unitOfWork.Payments.GetAll()
                .Include(p => p.RentalContract)
                .Where(p => p.RentalContractId == rentalContractId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(int paymentId, int employeeId)
        {
            return await _unitOfWork.Payments.GetAll()
                .Include(p => p.RentalContract)
                .ThenInclude(r => r.Customer)
                .FirstOrDefaultAsync(p => p.Id == paymentId);
        }

        public async Task<List<Payment>> GetAllEmployeesPaymentsAsync(int employeeId)
        {
            return await _unitOfWork.Payments.GetAll()
                .Include(p => p.RentalContract)
                .ThenInclude(r => r.Customer)
                .Where(p => p.RentalContract.EmployeeId == employeeId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetAllPaymentsAsync()
        {
            return await _unitOfWork.Payments.GetAll()
                .Include(p => p.RentalContract)
                .ThenInclude(r => r.Customer)
                .Include(p => p.RentalContract.Employee)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }
    }
}
