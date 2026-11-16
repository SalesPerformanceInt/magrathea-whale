using Whale.Api.Models;

namespace Whale.Api.Data;

public static class SeedData
{
    public static readonly IReadOnlyList<Customer> Customers =
    [
        new(1, "Ann.Lee@example.com", "Ann Lee", "617-555-0142"),
        new(2, "raj.patel@example.com", "Raj Patel", "978-555-0187"),
        new(3, "maria.gomez@example.com", "Maria Gomez", "508-555-0119"),
    ];

    public static IReadOnlyList<Tour> Tours(DateOnly firstDay, int days = 7)
    {
        var tours = new List<Tour>();
        for (var day = 0; day < days; day++)
        {
            var date = firstDay.AddDays(day);
            var baseId = int.Parse(date.ToString("yyyyMMdd")) * 10;
            tours.Add(new Tour(baseId + 1, "Morning Whale Watch", Harbor.At(date, new TimeOnly(10, 0)), 40, 45.00m, 30.00m));
            tours.Add(new Tour(baseId + 2, "Harbor Sunset Cruise", Harbor.At(date, new TimeOnly(17, 30)), 20, 52.00m, 32.00m));
        }
        return tours;
    }

    public static InMemoryBookingStore CreateStore(TimeProvider time)
    {
        var today = Harbor.Today(time);
        var tours = Tours(today);
        var now = time.GetUtcNow();
        var bookings = new List<Booking>
        {
            new(1, tours[1].Id, 1, 2, 1, now.AddDays(-3)),
            new(2, tours[2].Id, 2, 4, 0, now.AddDays(-1)),
            new(3, tours[3].Id, 3, 2, 2, now.AddHours(-5)),
        };
        return new InMemoryBookingStore(tours, bookings);
    }
}
