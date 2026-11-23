using Whale.Api.Data;
using Whale.Api.Services;

namespace Whale.Api.Tests;

// TEST1-30: Evening staff see the wrong day's tours.
// After about 7pm harbor time (America/New_York), the departures list shows
// tomorrow's morning tour instead of tonight's departures.
[TestClass]
public sealed class TourScheduleEveningTests
{
    private sealed class FrozenClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now.ToUniversalTime();
    }

    [TestMethod]
    public void TEST1_30_Today_InTheEvening_StillListsTodaysTours()
    {
        // Monday 2026-11-16, 8pm at the harbor (America/New_York, EST = UTC-5),
        // which is already Tuesday 01:00 in UTC.
        var monday = new DateOnly(2026, 11, 16);
        var eveningClock = new FrozenClock(Harbor.At(monday, new TimeOnly(20, 0)));

        var store = SeedData.CreateStore(eveningClock);
        var schedule = new TourSchedule(store, eveningClock);

        var today = schedule.Today();

        // The list must still show Monday's tours (ids 202611161/202611162),
        // not Tuesday morning's (202611171/202611172).
        CollectionAssert.AreEqual(
            new[] { 202611161, 202611162 },
            today.Select(t => t.Id).ToArray(),
            "After 7pm harbor time the departures list should still show today's tours, not tomorrow's.");
    }
}
