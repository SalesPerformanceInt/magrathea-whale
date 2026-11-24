using Whale.Api.Data;
using Whale.Api.Services;

namespace Whale.Api.Tests;

[TestClass]
public sealed class Test1_31_EveningDeparturesTests
{
    private sealed class FixedClock(DateTimeOffset now) : TimeProvider
    {
        public override DateTimeOffset GetUtcNow() => now.ToUniversalTime();
    }

    // TEST1-31: after about 7 PM at the harbor, the departures list shows
    // tomorrow's tours instead of tonight's.
    [TestMethod]
    public void TEST1_31_Today_InTheEvening_StillListsTodaysTours()
    {
        var monday = new DateOnly(2026, 11, 16);
        // 8 PM harbor time on Monday (EST, UTC-5) - UTC is already Tuesday.
        var clock = new FixedClock(Harbor.At(monday, new TimeOnly(20, 0)));
        var store = SeedData.CreateStore(clock);
        var schedule = new TourSchedule(store, clock);

        var today = schedule.Today();

        // Expected: Monday's tours (ids 202611161, 202611162), not Tuesday's.
        CollectionAssert.AreEqual(
            new[] { 202611161, 202611162 },
            today.Select(t => t.Id).ToArray());
    }
}
