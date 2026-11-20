using Whale.Api.Data;
using Whale.Api.Models;
using Whale.Api.Services;

namespace Whale.Api.Tests;

// TEST1-26: a customer opened the booking page at 10:15, was offered the
// 10:00 Morning Whale Watch, and booking it failed with "This tour has
// already departed." The booking refusal is intended; the defect is that
// the page's tour list (GET /tours/today -> TourSchedule.Today) still
// offers a tour after it has departed.
[TestClass]
public sealed class Test1_26_DepartedTourStillOfferedTests
{
    private sealed class QuarterPastTenTime : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() =>
            Harbor.At(new DateOnly(2026, 11, 16), new TimeOnly(10, 15)).ToUniversalTime();
    }

    [TestMethod]
    public void TEST1_26_Today_DoesNotOfferToursThatAlreadyDeparted()
    {
        var time = new QuarterPastTenTime();
        var store = SeedData.CreateStore(time);
        var schedule = new TourSchedule(store, time);

        var offered = schedule.Today().Select(t => t.Id).ToArray();

        CollectionAssert.AreEqual(new[] { 202611162 }, offered,
            "At 10:15 the 10:00 Morning Whale Watch (202611161) has departed and must not " +
            "be offered on the booking page; only the 17:30 Harbor Sunset Cruise should be.");
    }

    [TestMethod]
    public async Task TEST1_26_EveryTourOfferedForToday_CanBeBooked()
    {
        var time = new QuarterPastTenTime();
        var store = SeedData.CreateStore(time);
        var schedule = new TourSchedule(store, time);
        var service = new BookingService(store, schedule, new NoopPayments(), time);

        foreach (var offered in schedule.Today())
        {
            var booking = await service.BookAsync(1, new BookingRequest(offered.Id, 1, 0));
            Assert.AreEqual(offered.Id, booking.TourId,
                $"Tour {offered.Id} was offered on the booking page but could not be booked.");
        }
    }

    private sealed class NoopPayments : IPaymentGateway
    {
        public Task<string> AuthorizeAsync(int customerId, decimal amount, CancellationToken cancellationToken = default) =>
            Task.FromResult("auth_test");
    }
}
