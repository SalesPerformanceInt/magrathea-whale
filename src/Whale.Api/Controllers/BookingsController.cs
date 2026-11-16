using Microsoft.AspNetCore.Mvc;
using Whale.Api.Data;
using Whale.Api.Models;
using Whale.Api.Services;

namespace Whale.Api.Controllers;

public sealed record BookingDetails(
    int Id,
    int TourId,
    string TourName,
    DateTimeOffset Departure,
    int Adults,
    int Children,
    string Status,
    decimal RefundAmount,
    string CustomerName,
    string CustomerPhone);

[ApiController]
[Route("bookings")]
public sealed class BookingsController(
    IBookingStore store,
    ICustomerDirectory customers,
    BookingService bookings,
    InvoiceService invoices) : CustomerControllerBase
{
    [HttpGet]
    public ActionResult<IEnumerable<BookingDetails>> Mine()
    {
        if (!TryGetCaller(out var caller))
        {
            return Unauthorized();
        }
        return Ok(store.BookingsForCustomer(caller).Select(Details));
    }

    [HttpGet("{id:int}")]
    public ActionResult<BookingDetails> Get(int id)
    {
        if (!TryGetCaller(out _))
        {
            return Unauthorized();
        }
        var booking = store.FindBooking(id);
        return booking is null ? NotFound() : Details(booking);
    }

    [HttpPost]
    public async Task<ActionResult<BookingDetails>> Create(BookingRequest request, CancellationToken cancellationToken)
    {
        if (!TryGetCaller(out var caller))
        {
            return Unauthorized();
        }
        if (customers.Find(caller) is null)
        {
            return Unauthorized();
        }
        try
        {
            var booking = await bookings.BookAsync(caller, request, cancellationToken);
            return CreatedAtAction(nameof(Get), new { id = booking.Id }, Details(booking));
        }
        catch (BookingException e)
        {
            return BadRequest(new { error = e.Message });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpPost("{id:int}/cancel")]
    public ActionResult<CancellationResult> Cancel(int id)
    {
        if (!TryGetCaller(out var caller))
        {
            return Unauthorized();
        }
        try
        {
            return bookings.Cancel(caller, id);
        }
        catch (BookingException e)
        {
            return BadRequest(new { error = e.Message });
        }
        catch (NotFoundException)
        {
            return NotFound();
        }
    }

    [HttpGet("{id:int}/invoice")]
    public ActionResult<Invoice> Invoice(int id)
    {
        if (!TryGetCaller(out var caller))
        {
            return Unauthorized();
        }
        var booking = store.FindBooking(id);
        if (booking is null || booking.CustomerId != caller)
        {
            return NotFound();
        }
        return invoices.ForBooking(booking);
    }

    private BookingDetails Details(Booking booking)
    {
        var tour = store.FindTour(booking.TourId);
        var customer = customers.Find(booking.CustomerId);
        return new BookingDetails(
            booking.Id,
            booking.TourId,
            tour?.Name ?? "",
            tour?.Departure ?? default,
            booking.Adults,
            booking.Children,
            booking.Status.ToString(),
            booking.RefundAmount,
            customer?.Name ?? "",
            customer?.Phone ?? "");
    }
}
