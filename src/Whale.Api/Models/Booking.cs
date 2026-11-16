namespace Whale.Api.Models;

public enum BookingStatus
{
    Confirmed,
    Cancelled,
}

public sealed record Booking(
    int Id,
    int TourId,
    int CustomerId,
    int Adults,
    int Children,
    DateTimeOffset BookedAt,
    BookingStatus Status = BookingStatus.Confirmed,
    decimal RefundAmount = 0m)
{
    public int Seats => Adults + Children;
}
