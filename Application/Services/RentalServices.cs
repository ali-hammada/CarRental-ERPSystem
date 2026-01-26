using Application.Services.DTO_s;
using Application.Services.Interfaces;
using ApplicationCore.Entities;
using ApplicationCore.Entities.Enums;
using ApplicationCore.Interfaces;
using ApplicationCore.Interfaces.Repositories;

namespace Application.Services
{
  public class RentalServices:IRentalServices
  {
    private readonly ICarRepository _carRepo;
    private readonly IGenericRepository<RentalContract> _rentalRepo;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IRentalContractRepository _rentalContract;
    public RentalServices(ICarRepository CarRepo,IGenericRepository<RentalContract> RentalRepo,IUnitOfWork unitOfWork,IRentalContractRepository rentalContract)
    {
      _carRepo=CarRepo;
      _rentalRepo=RentalRepo;
      _unitOfWork=unitOfWork;
      _rentalContract=rentalContract;
    }

    //open contract 
    public async Task<(bool Success, string Content, int id)> OpenRequestRentalAsync(RentalRequestDTO request,int CustomerId)
    {
      var car = await _carRepo.GetByIdAsync(request.CarId);

      // Car Exsitanse 
      if(car==null||car.Status!=CarStatus.Available)
        return (false, "This Car is not Available", 0);

      //Date Validation
      if(request.StartDate<DateTime.Now.Date)
        return (false, "Start Date can't be in the past!..", 0);

      if(request.EndDate<=request.StartDate)
        return (false, "End date must be after start date.", 0);

      // Conflict reservied Car in the future 
      bool HasConflict = await _rentalContract.HasActiveRentalAsync(request.CarId,request.StartDate,request.EndDate);
      if(HasConflict)
        return (false, "This Car Is already reservied..  ", 0);


      // Calculate rent details
      int numberOfDays = (request.EndDate-request.StartDate).Days+1;
      decimal dailyPrice = car.PricePerDay;
      decimal totalAmount = dailyPrice*numberOfDays;

      //new Rent Contract 
      var rental = new RentalContract
      {
        CarId=request.CarId,
        CustomerId=CustomerId,
        StartDate=request.StartDate,
        EndDate=request.EndDate,
        DailyPrice=dailyPrice,
        TotalAmount=totalAmount,
        Status=RentalContractStatus.Open,
        Notes=request.Notes
      };


      await _rentalRepo.AddAsync(rental);
      car.Status=CarStatus.Rented;
      _carRepo.Update(car);
      await _unitOfWork.SaveChangesAsync();
      return (true, "", rental.Id);

    }

    public async Task<(bool Success, string Content, int id)> CancelRentalAsync(int rentalId,int customerId,string? reason = null)
    {
      var contract = await _rentalRepo.GetByIdAsync(rentalId);
      if(contract==null)
        return (false, "Contract doesn't exist", 0);

      if(contract.CustomerId!=customerId)
        return (false, "You are not allowed to cancel this contract", 0);

      if(contract.Status!=RentalContractStatus.Open)
        return (false, "Only Open contracts can be cancelled", 0);

      var car = await _carRepo.GetByIdAsync(contract.CarId);
      if(car==null)
        return (false, "Car does not exist", 0);

      decimal cancellationFee = 0;
      DateTime now = DateTime.Now;

      // قبل بداية العقد
      if(now.Date<contract.StartDate.Date)
      {
        int daysUntilStart = (contract.StartDate.Date-now.Date).Days;

        if(daysUntilStart<1)
          cancellationFee=contract.TotalAmount*0.5m;
        else if(daysUntilStart<3)
          cancellationFee=contract.TotalAmount*0.25m;
        else if(daysUntilStart<7)
          cancellationFee=contract.TotalAmount*0.15m;
        else cancellationFee=0;
      }
      // بعد بداية العقد
      else
      {
        int usedDays = (now.Date-contract.StartDate.Date).Days+1;
        decimal usedAmount = usedDays*contract.DailyPrice;

        // المبلغ المستخدم + 20% من الباقي كغرامة
        decimal remainingAmount = contract.TotalAmount-usedAmount;
        cancellationFee=usedAmount+(remainingAmount*0.2m);
      }

      contract.Status=RentalContractStatus.Cancelled;
      contract.ActualEndDate=now;
      contract.ExtraFees=cancellationFee;
      contract.FinalAmount=cancellationFee;

      if(!string.IsNullOrWhiteSpace(reason))
        contract.Notes+=$"\n[Cancelled: {now:yyyy-MM-dd}] Reason: {reason}";

      car.Status=CarStatus.Available;

      _rentalRepo.Update(contract);
      _carRepo.Update(car);
      await _unitOfWork.SaveChangesAsync();

      string message = cancellationFee>0
          ? $"Contract cancelled. Cancellation fee: {cancellationFee:C}"
          : "Contract cancelled successfully with no fees";

      return (true, message, rentalId);
    }
    // Extend the contract 
    public async Task<(bool Success, string Content, int id)> ExtendContractAsync(ExtendRentalDto extend,int customerId)
    {
      var existingContract = await _rentalRepo.GetByIdAsync(extend.RentalId);
      if(existingContract==null)
        return (false, "Contract doesn't exist", 0);

      if(existingContract.Status!=RentalContractStatus.Open)
        return (false, "Only Open contracts can be extended", 0);

      if(existingContract.CustomerId!=customerId)
        return (false, "You are not allowed to modify this contract", 0);

      if(!extend.NewEndDate.HasValue||extend.NewEndDate.Value<=existingContract.EndDate)
        return (false, "New end date must be after current end date", 0);

      bool hasConflict = await _rentalContract.HasActiveRentalAsync(
          existingContract.CarId,
          existingContract.EndDate.AddDays(1),
          extend.NewEndDate.Value);

      if(hasConflict)
        return (false, "Car is reserved during the extension period", 0);

      // حساب الأيام والمبلغ الإضافي
      int extraDays = (extend.NewEndDate.Value-existingContract.EndDate).Days;
      decimal extraAmount = extraDays*existingContract.DailyPrice;

      existingContract.EndDate=extend.NewEndDate.Value;
      existingContract.TotalAmount+=extraAmount;

      if(!string.IsNullOrWhiteSpace(extend.Notes))
        existingContract.Notes+=$"\n[Extended: {DateTime.Now:yyyy-MM-dd}] {extend.Notes}";

      _rentalRepo.Update(existingContract);
      await _unitOfWork.SaveChangesAsync();

      return (true, $"Contract extended by {extraDays} days. Extra amount: {extraAmount:C}", existingContract.Id);
    }



    //Close contract Function 
    public async Task<(bool Success, string Content, int id)> CloseContractAsync(RentalCloseDto request,int customerId)
    {
      var rental = await _rentalRepo.GetByIdAsync(request.RentalId);
      if(rental==null)
        return (false, "Contract doesn't exist", 0);

      if(rental.Status!=RentalContractStatus.Open)
        return (false, "Only OPEN contracts can be closed", 0);

      if(rental.CustomerId!=customerId)
        return (false, "You are not allowed to close this contract", 0);

      var car = await _carRepo.GetByIdAsync(rental.CarId);
      if(car==null)
        return (false, "Car doesn't exist", 0);

      rental.ActualEndDate=DateTime.Now;
      int actualDays = (rental.ActualEndDate.Value.Date-rental.StartDate.Date).Days+1;
      int expectedDays = (rental.EndDate-rental.StartDate.Date).Days+1;

      // Calculate base amount for actual days used
      decimal baseAmount = actualDays*rental.DailyPrice;
      rental.ExtraFees=0;

      // Apply late fee if returned late
      if(actualDays>expectedDays)
      {
        int lateDays = actualDays-expectedDays;
        rental.ExtraFees=lateDays*rental.DailyPrice*0.2m;
      }

      rental.FinalAmount=baseAmount+rental.ExtraFees.Value;
      rental.Status=RentalContractStatus.Closed;

      car.Status=CarStatus.Available;

      _rentalRepo.Update(rental);
      _carRepo.Update(car);
      await _unitOfWork.SaveChangesAsync();

      return (true, "Contract closed successfully", rental.Id);
    }



    public async Task<bool> HasActiveRentalAsync(int carId,DateTime start,DateTime end)
    {
      return await _rentalContract.HasActiveRentalAsync(carId,start,end);

    }

    public async Task<RentalContract> GetRentalByIdAsync(int reantalId)
    {
      return await _rentalContract.GetByIdAsync(reantalId);
    }

    public async Task<List<RentalContract>> GetCustomerRentalsAsync(int customerId)
    {
      var allRentals = _rentalRepo.GetAll();

      return allRentals
          .Where(r => r.CustomerId==customerId)
          .OrderByDescending(r => r.StartDate)
          .ToList();
    }
  }
}
