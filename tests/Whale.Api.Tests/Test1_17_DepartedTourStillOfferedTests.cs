using Whale.Api.Data;
using Whale.Api.Models;
using Whale.Api.Services;

namespace Whale.Api.Tests;

// TEST1-17: a customer opened the booking page at 10:15, was offered the
// 10:00 Morning Whale Watch, and booking it failed with "This tour has
// already departed." The schedule behind the booking page must not offer
// tours whose departure has already passed.
[TestClass]
public sealed class Test1_17_DepartedTourStillOfferedTests
{
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now.ToUniversalTime();
    }

    [TestMethod]
    public void TEST1_17_Today_DoesNotOfferToursThatAlreadyDeparted()
    {
        var day = new DateOnly(2026, 11, 16);
        var morning = new Tour(1, "Morning Whale Watch", Harbor.At(day, new TimeOnly(10, 0)), 40, 45.00m, 30.00m);
        var sunset = new Tour(2, "Harbor Sunset Cruise", Harbor.At(day, new TimeOnly(17, 30)), 20, 52.00m, 32.00m);
        var store = new InMemoryBookingStore([morning, sunset]);

        // It is 10:15 at the harbor: the 10:00 whale watch departed 15 minutes ago.
        var quarterPastTen = new FixedClock(Harbor.At(day, new TimeOnly(10, 15)));
        var schedule = new TourSchedule(store, quarterPastTen);

        var offered = schedule.Today();

        // The booking page should still offer the 17:30 sunset cruise...
        CollectionAssert.Contains(offered.Select(t => t.Id).ToList(), sunset.Id);
        // ...but not the already-departed 10:00 whale watch, which can only
        // produce a "This tour has already departed." error when booked.
        CollectionAssert.DoesNotContain(
            offered.Select(t => t.Id).ToList(),
            morning.Id,
            "The 10:00 tour had already departed at 10:15 and must not be offered for booking.");
    }
}
