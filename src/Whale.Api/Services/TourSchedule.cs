using Whale.Api.Data;
using Whale.Api.Models;

namespace Whale.Api.Services;

public sealed record TourAvailability(int Id, string Name, DateTimeOffset Departure, int SeatsLeft, decimal AdultPrice, decimal ChildPrice);

public sealed class TourSchedule(IBookingStore store, TimeProvider time)
{
    public int SeatsTaken(int tourId) =>
        store.BookingsForTour(tourId).Where(b => b.Status == BookingStatus.Confirmed).Sum(b => b.Seats);

    public TourAvailability Availability(Tour tour) =>
        new(tour.Id, tour.Name, tour.Departure, tour.Capacity - SeatsTaken(tour.Id), tour.AdultPrice, tour.ChildPrice);

    public IReadOnlyList<TourAvailability> Today()
    {
        var today = time.GetUtcNow().Date;
        return store.Tours()
            .Where(t => t.Departure.Date == today)
            .Select(Availability)
            .ToList();
    }
}
