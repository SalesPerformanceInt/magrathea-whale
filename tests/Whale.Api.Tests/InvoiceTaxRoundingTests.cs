using Moq;
using Whale.Api.Data;
using Whale.Api.Models;
using Whale.Api.Services;

namespace Whale.Api.Tests;

[TestClass]
public sealed class InvoiceTaxRoundingTests
{
    // TEST1-11: Invoice tax a penny off.
    // 2 adults on the Morning Whale Watch ($45.00 each) = $90.00 subtotal.
    // 6.25% of $90.00 is $5.625, which should round half-up to $5.63,
    // but the invoice shows $5.62.

    private static Tour MorningWhaleWatch() =>
        new(1, "Morning Whale Watch", Harbor.At(new DateOnly(2026, 11, 16), new TimeOnly(10, 0)), 40, 45.00m, 30.00m);

    [TestMethod]
    public void TEST1_11_TaxOn_RoundsHalfCentUp()
    {
        Assert.AreEqual(5.63m, InvoiceService.TaxOn(90.00m));
    }

    [TestMethod]
    public void TEST1_11_ForBooking_TwoAdultsMorningWhaleWatch_TaxIsFiveSixtyThree()
    {
        var morning = MorningWhaleWatch();
        var store = new InMemoryBookingStore([morning]);
        var customers = new Mock<ICustomerDirectory>();
        customers.Setup(c => c.Find(7)).Returns(new Customer(7, "user@example.com", "Test User", ""));
        var service = new InvoiceService(store, customers.Object);

        var invoice = service.ForBooking(new Booking(55, morning.Id, 7, 2, 0, DateTimeOffset.UnixEpoch));

        Assert.AreEqual(90.00m, invoice.Subtotal);
        Assert.AreEqual(5.63m, invoice.Tax);
        Assert.AreEqual(95.63m, invoice.Total);
    }
}
