using Whale.Api.Data;
using Whale.Api.Services;

namespace Whale.Api.Tests;

[TestClass]
public sealed class TourScheduleEveningTests
{
    // TEST1-12: "Today's tours" shows tomorrow in the evening.
    // At 8 PM harbor time it is already past midnight UTC, so the schedule
    // must still use the harbor's local date, not the UTC date.
    [TestMethod]
    public void TEST1_12_Today_AtEightPmLocal_StillListsTodaysDepartures()
    {
        var monday = new DateOnly(2026, 11, 16);
        var eightPmMonday = new FixedTime(Harbor.At(monday, new TimeOnly(20, 0)));
        var store = SeedData.CreateStore(eightPmMonday);
        var schedule = new TourSchedule(store, eightPmMonday);

        var today = schedule.Today();

        // Monday's tours: 202611161 (10:00 whale watch) and 202611162 (17:30 sunset cruise).
        CollectionAssert.AreEqual(new[] { 202611161, 202611162 }, today.Select(t => t.Id).ToArray(),
            "At 8 PM local the Today's Tours list must still show today's departures, not tomorrow's.");
    }
}
