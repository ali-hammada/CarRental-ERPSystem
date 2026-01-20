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
      var HasConflict = await _rentalContract.HasActiveRentalAsync(request.CarId,request.StartDate,request.EndDate);
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
        rental.ExtraFees=lateDays*rental.DailyPrice*0.2m; // 20% penalty
      }

      rental.FinalAmount=baseAmount+rental.ExtraFees.Value;
      rental.Status=RentalContractStatus.Closed;

      car.Status=CarStatus.Available;

      _rentalRepo.Update(rental);
      _carRepo.Update(car); // Important!
      await _unitOfWork.SaveChangesAsync();

      return (true, "Contract closed successfully", rental.Id);
    }



    public async Task<bool> HasActiveRentalAsync(int carId,DateTime start,DateTime end)
    {
      return await _rentalContract.HasActiveRentalAsync(carId,start,end);

    }

    public async Task<(bool Success, string Content, int id)> ExtendContractAsync(ExtendRentalDto extend,int customerId)
    {
      // العقد موجود قبل كدا ولا لا 
      var ExistingContract = await _rentalRepo.GetByIdAsync(extend.RentalId);
      if(ExistingContract==null)
        return (false, "Contract Doesn't yet Exists", 0);
      //حاله العقد مفتوح ولا مقفول 
      if(ExistingContract.Status!=RentalContractStatus.Open)
        return (false, "Contract Must be Open at First !.. ", 0);
      // التاريخ الجديد لازم يكون بعد تاريخ النهايه القديم 
      if(ExistingContract.EndDate>=extend.NewEndDate)
        return (false, "New end date must be after current end date.", 0);
      // لازم ابعتله التاريخ الجديد للعقد 
      if(!extend.NewEndDate.HasValue)
        return (false, "New end date must be provided.", 0);
      // صاحب العقد بس اللي يقدر يعمله امتداد 
      if(ExistingContract.CustomerId!=customerId)
        return (false, "You are not allowed to modify this contract.", 0);
      //تعديل علي الفتره الجديده للعقد وحساب المستحق حسب الايام 
      var extraDays = (extend.NewEndDate.Value-ExistingContract.EndDate).Days;
      decimal ExtraAmount = extraDays*ExistingContract.DailyPrice;
      return (true, $"Contract extended by {extraDays} days. Extra amount: {ExtraAmount}", ExistingContract.Id);

    }



  }
}
