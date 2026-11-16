using Whale.Api.Data;
using Whale.Api.Models;

namespace Whale.Api.Tests;

internal sealed class FixedTime(DateTimeOffset now) : TimeProvider
{
    public DateTimeOffset Now { get; set; } = now;

    public override DateTimeOffset GetUtcNow() => Now.ToUniversalTime();
}

internal static class TestData
{
    public static readonly DateOnly Monday = new(2026, 11, 16);

    public static Tour Morning(DateOnly date, int capacity = 40) =>
        new(1, "Morning Whale Watch", Harbor.At(date, new TimeOnly(10, 0)), capacity, 45.00m, 30.00m);

    public static Tour Sunset(DateOnly date, int capacity = 20) =>
        new(2, "Harbor Sunset Cruise", Harbor.At(date, new TimeOnly(17, 30)), capacity, 52.00m, 32.00m);
}
