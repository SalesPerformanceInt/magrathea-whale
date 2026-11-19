using Whale.Api.Data;
using Whale.Api.Models;
using Whale.Api.Services;

namespace Whale.Api.Tests;

/// <summary>
/// TEST1-10: the Harbor Sunset Cruise (capacity 20) ended up with 22 confirmed
/// seats after two families booked within a minute of each other. Two concurrent
/// bookings both pass the seats-left check before either one is saved.
/// </summary>
[TestClass]
public sealed class OverbookingTests
{
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now.ToUniversalTime();
    }

    /// <summary>
    /// A gateway that lets the test hold both booking requests at the payment
    /// step, so both have already passed the capacity check before either
    /// booking is stored. This mirrors the real FakePaymentGateway's delay.
    /// </summary>
    private sealed class GatedPaymentGateway : IPaymentGateway
    {
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _bothArrived = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _arrived;

        public Task BothArrived => _bothArrived.Task;

        public void Release() => _release.TrySetResult();

        public async Task<string> AuthorizeAsync(int customerId, decimal amount, CancellationToken cancellationToken = default)
        {
            if (Interlocked.Increment(ref _arrived) == 2)
            {
                _bothArrived.TrySetResult();
            }
            await _release.Task;
            return $"auth_test_{customerId}";
        }
    }

    [TestMethod]
    public async Task TEST1_10_ConcurrentBookings_DoNotExceedTourCapacity()
    {
        var monday = new DateOnly(2026, 11, 16);
        var sunset = new Tour(2, "Harbor Sunset Cruise", Harbor.At(monday, new TimeOnly(17, 30)), Capacity: 20, 52.00m, 32.00m);
        // 18 seats already confirmed: only 2 are left when the two families book.
        var existing = new Booking(1, sunset.Id, 5, 18, 0, Harbor.At(monday, new TimeOnly(7, 0)));
        var store = new InMemoryBookingStore([sunset], [existing]);
        var time = new FixedClock(Harbor.At(monday, new TimeOnly(8, 0)));
        var gateway = new GatedPaymentGateway();
        var service = new BookingService(store, new TourSchedule(store, time), gateway, time);

        // Two families book the last seats at (almost) the same moment.
        var firstFamily = service.BookAsync(1, new BookingRequest(sunset.Id, 2, 0));
        var secondFamily = service.BookAsync(2, new BookingRequest(sunset.Id, 2, 0));

        // Wait until both requests sit in payment authorization (i.e. both have
        // already passed the seats-left check), then let them finish. If the
        // service serializes bookings correctly, only one reaches the gateway;
        // release after a grace period so the test does not hang in that case.
        await Task.WhenAny(gateway.BothArrived, Task.Delay(TimeSpan.FromSeconds(2)));
        gateway.Release();

        var outcomes = await Task.WhenAll(AsOutcome(firstFamily), AsOutcome(secondFamily));

        var confirmedSeats = store.BookingsForTour(sunset.Id)
            .Where(b => b.Status == BookingStatus.Confirmed)
            .Sum(b => b.Seats);
        Assert.IsTrue(
            confirmedSeats <= sunset.Capacity,
            $"Tour has {confirmedSeats} confirmed seats but is licensed for {sunset.Capacity}. " +
            $"Outcomes: [{string.Join(", ", outcomes)}]");
    }

    private static async Task<string> AsOutcome(Task<Booking> booking)
    {
        try
        {
            var saved = await booking;
            return $"booked {saved.Seats} seats";
        }
        catch (BookingException e)
        {
            return $"refused: {e.Message}";
        }
    }
}
