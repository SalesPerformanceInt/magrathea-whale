using Whale.Api.Models;

namespace Whale.Api.Data;

public interface IBookingStore
{
    IReadOnlyList<Tour> Tours();
    Tour? FindTour(int tourId);
    Booking? FindBooking(int bookingId);
    IReadOnlyList<Booking> BookingsForTour(int tourId);
    IReadOnlyList<Booking> BookingsForCustomer(int customerId);
    Booking AddBooking(Booking booking);
    void UpdateBooking(Booking booking);
}
