using Whale.Api.Data;
using Whale.Api.Models;
using Whale.Api.Services;

namespace Whale.Api.Tests;

[TestClass]
public sealed class TourScheduleEveningTests
{
    private sealed class FixedEveningTime(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now.ToUniversalTime();
    }

    // TEST1-29: Evening staff see the wrong day's tours.
    // After about 7pm harbor time the departures list shows tomorrow's
    // morning tour instead of tonight's, because the schedule compares
    // against the UTC date, which has already rolled over to tomorrow.
    [TestMethod]
    public void TEST1_29_Today_InTheEvening_StillListsTonightsTours_NotTomorrowsMorning()
    {
        var monday = new DateOnly(2026, 11, 16);
        var tuesday = monday.AddDays(1);

        var mondaySunset = new Tour(1, "Harbor Sunset Cruise", Harbor.At(monday, new TimeOnly(17, 30)), 20, 52.00m, 32.00m);
        var tuesdayMorning = new Tour(2, "Morning Whale Watch", Harbor.At(tuesday, new TimeOnly(10, 0)), 40, 45.00m, 30.00m);
        var store = new InMemoryBookingStore([mondaySunset, tuesdayMorning], []);

        // Monday 8:00pm at the harbor (America/New_York, EST): the UTC
        // date is already Tuesday, but staff should still see Monday's tours.
        var eveningClock = new FixedEveningTime(Harbor.At(monday, new TimeOnly(20, 0)));
        var schedule = new TourSchedule(store, eveningClock);

        var today = schedule.Today();

        CollectionAssert.AreEqual(new[] { mondaySunset.Id }, today.Select(t => t.Id).ToArray(),
            "The evening departures list should show tonight's tours, not tomorrow's morning tour.");
    }
}
