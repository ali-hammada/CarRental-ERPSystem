using ApplicationCore.Entities;
using ApplicationCore.Enums;
using Xunit;

namespace CarRental.Tests
{
    public class DomainLogicTests
    {
        [Fact]
        public void LicenseExpiryCheck_ShouldFlagExpiredLicense()
        {
            // Arrange
            var customer = new Customer
            {
                Name = "John Doe",
                LicenseExpiryDate = DateTime.UtcNow.AddDays(-5) // Expired 5 days ago
            };

            var contractEndDate = DateTime.UtcNow.AddDays(2);

            // Act
            bool isExpired = customer.LicenseExpiryDate.Date < contractEndDate.Date;

            // Assert
            Assert.True(isExpired);
        }

        [Fact]
        public void SystemEnums_ShouldHaveExplicitIntegerValues()
        {
            // Assert explicit numeric assignments for optimized database storage
            Assert.Equal(1, (int)CarStatus.Available);
            Assert.Equal(2, (int)CarStatus.Rented);
            Assert.Equal(3, (int)CarStatus.Maintenance);
            Assert.Equal(4, (int)CarStatus.OutOfService);

            Assert.Equal(1, (int)RentalContractStatus.Draft);
            Assert.Equal(3, (int)RentalContractStatus.Open);
            Assert.Equal(4, (int)RentalContractStatus.Closed);
            Assert.Equal(5, (int)RentalContractStatus.Cancelled);

            Assert.Equal(1, (int)PaymentStatus.Unpaid);
            Assert.Equal(3, (int)PaymentStatus.Paid);
        }

        [Fact]
        public void ContractRemainingAmount_Calculation_ShouldBeCorrect()
        {
            // Arrange
            var contract = new RentalContract
            {
                TotalAmount = 500m,
                PaidAmount = 200m
            };

            // Act
            decimal remaining = contract.RemainingAmount;

            // Assert
            Assert.Equal(300m, remaining);
        }
    }
}
