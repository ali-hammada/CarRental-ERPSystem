namespace ApplicationCore.Enums
{
    public enum CarStatus
    {
        Available = 1,
        Rented = 2,
        Maintenance = 3,
        OutOfService = 4
    }

    public enum RentalContractStatus
    {
        Draft = 1,
        Reserved = 2,
        Open = 3,
        Closed = 4,
        Cancelled = 5,
        Overdue = 6
    }

    public enum PaymentStatus
    {
        Unpaid = 1,
        PartiallyPaid = 2,
        Paid = 3,
        Failed = 4,
        Refunded = 5
    }

    public enum PaymentMethod
    {
        Cash = 1,
        CreditCard = 2,
        DebitCard = 3,
        BankTransfer = 4,
        OnlineGateway = 5
    }

    public enum PaymentPurpose
    {
        Deposit = 1,
        Partial = 2,
        Final = 3,
        Penalty = 4,
        Refund = 5
    }
}
