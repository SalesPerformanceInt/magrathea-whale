using System.Collections.Concurrent;
using Whale.Api.Models;

namespace Whale.Api.Data;

public sealed class InMemoryBookingStore : IBookingStore
{
    private readonly ConcurrentDictionary<int, Tour> _tours = new();
    private readonly ConcurrentDictionary<int, Booking> _bookings = new();
    private int _nextBookingId = 1000;

    public InMemoryBookingStore(IEnumerable<Tour> tours, IEnumerable<Booking>? bookings = null)
    {
        foreach (var tour in tours)
        {
            _tours[tour.Id] = tour;
        }
        foreach (var booking in bookings ?? [])
        {
            _bookings[booking.Id] = booking;
            _nextBookingId = Math.Max(_nextBookingId, booking.Id);
        }
    }

    public IReadOnlyList<Tour> Tours() => _tours.Values.OrderBy(t => t.Departure).ToList();

    public Tour? FindTour(int tourId) => _tours.GetValueOrDefault(tourId);

    public Booking? FindBooking(int bookingId) => _bookings.GetValueOrDefault(bookingId);

    public IReadOnlyList<Booking> BookingsForTour(int tourId) =>
        _bookings.Values.Where(b => b.TourId == tourId).OrderBy(b => b.Id).ToList();

    public IReadOnlyList<Booking> BookingsForCustomer(int customerId) =>
        _bookings.Values.Where(b => b.CustomerId == customerId).OrderByDescending(b => b.BookedAt).ToList();

    public Booking AddBooking(Booking booking)
    {
        var saved = booking with { Id = Interlocked.Increment(ref _nextBookingId) };
        _bookings[saved.Id] = saved;
        return saved;
    }

    public void UpdateBooking(Booking booking) => _bookings[booking.Id] = booking;
}
