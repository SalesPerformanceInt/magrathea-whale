using Whale.Api.Data;
using Whale.Api.Models;

namespace Whale.Api.Services;

public sealed class BookingService(IBookingStore store, TourSchedule schedule, IPaymentGateway payments, TimeProvider time)
{
    public async Task<Booking> BookAsync(int customerId, BookingRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Adults < 0 || request.Children < 0 || request.Adults + request.Children == 0)
        {
            throw new BookingException("Book at least one seat.");
        }
        if (request.Adults == 0)
        {
            throw new BookingException("Children must be booked with an adult.");
        }
        var tour = store.FindTour(request.TourId) ?? throw new NotFoundException($"Tour {request.TourId} not found.");
        var now = time.GetUtcNow();
        if (tour.Departure <= now)
        {
            throw new BookingException("This tour has already departed.");
        }
        var seatsLeft = tour.Capacity - schedule.SeatsTaken(tour.Id);
        if (request.Adults + request.Children > seatsLeft)
        {
            throw new BookingException($"Only {seatsLeft} seats left on this tour.");
        }

        await payments.AuthorizeAsync(customerId, InvoiceService.AmountDue(tour, request.Adults, request.Children), cancellationToken);

        return store.AddBooking(new Booking(0, tour.Id, customerId, request.Adults, request.Children, now));
    }

    public CancellationResult Cancel(int customerId, int bookingId)
    {
        var booking = store.FindBooking(bookingId);
        if (booking is null || booking.CustomerId != customerId)
        {
            throw new NotFoundException($"Booking {bookingId} not found.");
        }
        if (booking.Status == BookingStatus.Cancelled)
        {
            throw new BookingException("This booking is already cancelled.");
        }
        var tour = store.FindTour(booking.TourId) ?? throw new NotFoundException($"Tour {booking.TourId} not found.");
        var paid = InvoiceService.AmountDue(tour, booking.Adults, booking.Children);
        var notice = tour.Departure - time.GetUtcNow();

        decimal refund;
        if (notice >= TimeSpan.FromHours(48))
        {
            refund = paid;
        }
        else if (notice.Hours >= 24)
        {
            refund = Math.Round(paid / 2, 2, MidpointRounding.AwayFromZero);
        }
        else
        {
            refund = 0m;
        }

        store.UpdateBooking(booking with { Status = BookingStatus.Cancelled, RefundAmount = refund });
        return new CancellationResult(booking.Id, refund);
    }
}
