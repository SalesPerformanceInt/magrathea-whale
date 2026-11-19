using Moq;
using Whale.Api.Data;
using Whale.Api.Models;
using Whale.Api.Services;

namespace Whale.Api.Tests;

[TestClass]
public sealed class CancellationRefundTests
{
    private sealed class StoppedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now.ToUniversalTime();
    }

    [TestMethod]
    public void TEST1_15_CancellingBetween24And48HoursBeforeDepartureRefundsHalf()
    {
        // The customer cancelled 30 hours before departure. Policy: 50% refund
        // for cancellations between 24 and 48 hours before departure.
        var departure = Harbor.At(new DateOnly(2026, 11, 17), new TimeOnly(17, 30));
        var tour = new Tour(2, "Harbor Sunset Cruise", departure, 20, 52.00m, 32.00m);
        var booking = new Booking(10, tour.Id, 1, 2, 0, departure.AddDays(-7));
        var store = new InMemoryBookingStore([tour], [booking]);
        var time = new StoppedClock(departure.AddHours(-30));
        var payments = new Mock<IPaymentGateway>();
        var service = new BookingService(store, new TourSchedule(store, time), payments.Object, time);

        var result = service.Cancel(1, 10);

        var paid = InvoiceService.AmountDue(tour, 2, 0);
        var expectedRefund = Math.Round(paid / 2, 2, MidpointRounding.AwayFromZero);
        Assert.AreEqual(expectedRefund, result.RefundAmount,
            "cancelling 30 hours ahead should refund 50%, not $" + result.RefundAmount);
        Assert.AreEqual(expectedRefund, store.FindBooking(10)!.RefundAmount);
    }
}
