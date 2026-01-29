using Application.Services.Interfaces;
using ApplicationCore.Entities;
using ApplicationCore.Entities.Enums;
using ApplicationCore.Interfaces;
using Microsoft.EntityFrameworkCore;



namespace Application.Services
{
  public class PaymentServices:IPaymentServices
  {
    private readonly IGenericRepository<Payment> _payRepo;
    private readonly IGenericRepository<RentalContract> _rentalRepo;
    private readonly IUnitOfWork _unitOfWork;



    public PaymentServices(IGenericRepository<Payment> payRepo,IGenericRepository<RentalContract> rentalRepo,IUnitOfWork unitOfWork)
    {
      _payRepo=payRepo;
      _rentalRepo=rentalRepo;
      _unitOfWork=unitOfWork;
    }

    public async Task<(bool success, string message, int PaymentId)> MakePaymentAsync(int rentalContractId,decimal amount,PaymentPurpose purpose,PaymentMethod method,int employeeId)
    {
      var contract = await _rentalRepo.GetByIdAsync(rentalContractId);
      if(contract==null)
        return (false, "Contract does not exist", 0);

      if(amount<=0)
        return (false, "Payment amount must be greater than zero", 0);

      if(contract.EmployeeId!=employeeId)
        return (false, "Only the contract owner is allowed to make payments", 0);


      if(contract.Status!=RentalContractStatus.Draft&&contract.Status!=RentalContractStatus.Open)
        return (false, "Only Draft or Open contracts can accept payments", 0);

      decimal amountDue = contract.FinalAmount??contract.TotalAmount;
      decimal remainingBefore = amountDue-contract.PaidAmount;

      Payment payment;
      string message;

      switch(purpose)
      {
        case PaymentPurpose.Partial:

          if(contract.PaidAmount+amount>amountDue)
            return (false, $"Payment exceeds remaining amount of {remainingBefore:C}", 0);


          if(contract.PaidAmount+amount>=amountDue)
            return (false, "Use 'Final' payment type to complete the payment", 0);

          contract.PaidAmount+=amount;
          decimal remainingAfter = amountDue-contract.PaidAmount;

          payment=new Payment
          {
            RentalContractId=rentalContractId,
            Amount=amount,
            Purpose=purpose,
            Method=method,
            Status=PaymentStatus.Paid,
            PaymentDate=DateTime.UtcNow
          };

          await _payRepo.AddAsync(payment);
          contract.PaymentStatus=PaymentStatus.PartiallyPaid;
          _rentalRepo.Update(contract);
          await _unitOfWork.SaveChangesAsync();

          message=$"Partial payment of {amount:C} received. Remaining: {remainingAfter:C}";
          return (true, message, payment.Id);

        case PaymentPurpose.Final:

          if(amount<remainingBefore)
            return (false, $"Final payment must be at least {remainingBefore:C}", 0);


          decimal overpayment = amount-remainingBefore;

          contract.PaidAmount=amountDue;
          contract.PaymentStatus=PaymentStatus.Paid;


          if(contract.Status==RentalContractStatus.Draft)
            contract.Status=RentalContractStatus.Open; // عشان ماينفعش ادفع علي عقد مقفول بقفل العقد بعد الدفع مش قبله ياغبي 

          payment=new Payment
          {
            RentalContractId=rentalContractId,
            Amount=amount,
            Purpose=purpose,
            Method=method,
            Status=PaymentStatus.Paid,
            PaymentDate=DateTime.UtcNow
          };

          await _payRepo.AddAsync(payment);
          _rentalRepo.Update(contract);
          await _unitOfWork.SaveChangesAsync();

          message=overpayment>0
              ? $"Final payment completed. Change: {overpayment:C}"
              : $"Final payment of {amount:C} completed successfully";

          return (true, message, payment.Id);

        case PaymentPurpose.Penalty:

          contract.ExtraFees=(contract.ExtraFees??0)+amount;
          contract.FinalAmount=(contract.FinalAmount??contract.TotalAmount)+amount;
          contract.PaidAmount+=amount;

          payment=new Payment
          {
            RentalContractId=rentalContractId,
            Amount=amount,
            Purpose=purpose,
            Method=method,
            Status=PaymentStatus.Paid,
            PaymentDate=DateTime.UtcNow
          };

          await _payRepo.AddAsync(payment);
          _rentalRepo.Update(contract);
          await _unitOfWork.SaveChangesAsync();

          message=$"Penalty of {amount:C} added and paid. New total: {contract.FinalAmount:C}";
          return (true, message, payment.Id);

        default:
          return (false, "Invalid payment purpose", 0);

      }

    }
    public async Task<(bool success, string message, decimal remaining)> GetRemainingAmountAsync(int rentalContractId,int employeeId)
    {
      var contract = await _rentalRepo.GetByIdAsync(rentalContractId);
      if(contract==null)
        return (false, "Contract doesn't exist", 0);

      if(contract.EmployeeId!=employeeId)
        return (false, "Unauthorized", 0);

      decimal amountDue = contract.FinalAmount??contract.TotalAmount;
      decimal remaining = amountDue-contract.PaidAmount;

      return (true, $"Remaining amount: {remaining:C}", remaining);
    }

    public async Task<List<Payment>> GetContractPaymentsAsync(int rentalContractId,int employeeId)
    {
      var contract = await _rentalRepo.GetByIdAsync(rentalContractId);
      if(contract==null||contract.EmployeeId!=employeeId)
        return new List<Payment>();

      var allPayments = _payRepo.GetAll();
      return allPayments
          .Where(p => p.RentalContractId==rentalContractId)
          .OrderByDescending(p => p.PaymentDate)
          .ToList();
    }

    public async Task<Payment?> GetPaymentByIdAsync(int paymentId,int employeeId)
    {
      var payment = await _payRepo.GetByIdAsync(paymentId);
      if(payment==null)
        return null;

      var contract = await _rentalRepo.GetByIdAsync(payment.RentalContractId);
      if(contract==null||contract.EmployeeId!=employeeId)
        return null;

      return payment;
    }

    public async Task<List<Payment>> GetAllEmployeesPaymentsAsync(int employeeId)
    {
      var rentalIds =
          _rentalRepo.GetAll()
          .Where(r => r.EmployeeId==employeeId)
          .Select(r => r.Id);

      return await _payRepo.GetAll()
          .Where(p => rentalIds.Contains(p.RentalContractId))
          .OrderByDescending(p => p.PaymentDate)
          .ToListAsync();
    }
  }
}

