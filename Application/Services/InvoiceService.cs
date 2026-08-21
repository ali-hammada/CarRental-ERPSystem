using ApplicationCore.Entities;
using ApplicationCore.Enums;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Application.Services
{
    public interface IInvoiceService
    {
        Task<IEnumerable<Invoice>> GetAllInvoicesAsync();
        Task<Invoice?> GetByIdAsync(int id);
        Task<Invoice?> GetByContractIdAsync(int contractId);
        Task GenerateInvoiceForContractAsync(int contractId);
    }

    public class InvoiceService : IInvoiceService
    {
        private readonly IUnitOfWork _unitOfWork;

        public InvoiceService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Invoice>> GetAllInvoicesAsync()
        {
            return await _unitOfWork.Invoices.GetAll()
                .Include(i => i.RentalContract)
                .ThenInclude(r => r.Customer)
                .Include(i => i.RentalContract.Car)
                .OrderByDescending(i => i.IssueDate)
                .ToListAsync();
        }

        public async Task<Invoice?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Invoices.GetAll()
                .Include(i => i.RentalContract)
                .ThenInclude(r => r.Customer)
                .Include(i => i.RentalContract.Car)
                .Include(i => i.RentalContract.Employee)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        public async Task<Invoice?> GetByContractIdAsync(int contractId)
        {
            return await _unitOfWork.Invoices.GetAll()
                .Include(i => i.RentalContract)
                .ThenInclude(r => r.Customer)
                .Include(i => i.RentalContract.Car)
                .FirstOrDefaultAsync(i => i.RentalContractId == contractId);
        }

        public async Task GenerateInvoiceForContractAsync(int contractId)
        {
            var contract = await _unitOfWork.RentalContracts.GetByIdAsync(contractId);
            if (contract == null) return;

            var existingInvoice = await GetByContractIdAsync(contractId);
            if (existingInvoice != null) return;

            decimal subTotal = contract.FinalAmount ?? contract.TotalAmount;
            decimal taxRate = 0.14m;
            decimal taxAmount = Math.Round(subTotal * taxRate, 2);
            decimal totalAmount = Math.Round(subTotal + taxAmount, 2);

            var invoice = new Invoice
            {
                InvoiceNumber = $"INV-{DateTime.UtcNow:yyyyMMdd}-{contract.Id:D5}",
                RentalContractId = contractId,
                IssueDate = DateTime.UtcNow,
                SubTotal = subTotal,
                TaxRate = taxRate,
                TaxAmount = taxAmount,
                TotalAmount = totalAmount,
                PaidAmount = contract.PaidAmount,
                Status = contract.PaymentStatus == PaymentStatus.Paid ? "Paid" : "Issued"
            };

            await _unitOfWork.Invoices.AddAsync(invoice);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}
