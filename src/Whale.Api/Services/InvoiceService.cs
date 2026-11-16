using Whale.Api.Data;
using Whale.Api.Models;

namespace Whale.Api.Services;

public sealed class InvoiceService(IBookingStore store, ICustomerDirectory customers)
{
    public const decimal TaxRate = 0.0625m;

    public static decimal Subtotal(Tour tour, int adults, int children) =>
        adults * tour.AdultPrice + children * tour.ChildPrice;

    public static decimal TaxOn(decimal subtotal) => Math.Round(subtotal * TaxRate, 2);

    public static decimal AmountDue(Tour tour, int adults, int children)
    {
        var subtotal = Subtotal(tour, adults, children);
        return subtotal + TaxOn(subtotal);
    }

    public Invoice ForBooking(Booking booking)
    {
        var tour = store.FindTour(booking.TourId) ?? throw new NotFoundException($"Tour {booking.TourId} not found.");
        var customer = customers.Find(booking.CustomerId);
        var lines = new List<InvoiceLine>();
        if (booking.Adults > 0)
        {
            lines.Add(new InvoiceLine($"{tour.Name}, adult", booking.Adults, tour.AdultPrice, booking.Adults * tour.AdultPrice));
        }
        if (booking.Children > 0)
        {
            lines.Add(new InvoiceLine($"{tour.Name}, child", booking.Children, tour.ChildPrice, booking.Children * tour.ChildPrice));
        }
        var subtotal = Subtotal(tour, booking.Adults, booking.Children);
        var tax = TaxOn(subtotal);
        return new Invoice(booking.Id, customer?.Name ?? "", lines, subtotal, tax, subtotal + tax);
    }
}
