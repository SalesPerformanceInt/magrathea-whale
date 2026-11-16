using Whale.Api.Data;
using Whale.Api.Models;
using Whale.Api.Services;

namespace Whale.Api.Tests;

[TestClass]
public sealed class TourScheduleTests
{
    [TestMethod]
    public void Today_ListsTodaysDeparturesWithSeatsLeft()
    {
        var store = SeedData.CreateStore(new FixedTime(Harbor.At(TestData.Monday, new TimeOnly(9, 0))));
        var schedule = new TourSchedule(store, new FixedTime(Harbor.At(TestData.Monday, new TimeOnly(9, 0))));

        var today = schedule.Today();

        CollectionAssert.AreEqual(new[] { 202611161, 202611162 }, today.Select(t => t.Id).ToArray());
        Assert.AreEqual(40, today[0].SeatsLeft);
        Assert.AreEqual(17, today[1].SeatsLeft);
    }

    [TestMethod]
    public void SeatsTaken_CountsConfirmedBookingsOnly()
    {
        var tour = TestData.Sunset(TestData.Monday);
        var store = new InMemoryBookingStore([tour],
        [
            new Booking(1, tour.Id, 1, 2, 1, DateTimeOffset.UnixEpoch),
            new Booking(2, tour.Id, 2, 4, 0, DateTimeOffset.UnixEpoch, BookingStatus.Cancelled),
        ]);

        Assert.AreEqual(3, new TourSchedule(store, TimeProvider.System).SeatsTaken(tour.Id));
    }
}
