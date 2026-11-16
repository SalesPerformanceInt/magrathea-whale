using Moq;
using Whale.Api.Data;
using Whale.Api.Models;
using Whale.Api.Services;

namespace Whale.Api.Tests;

[TestClass]
public sealed class InvoiceServiceTests
{
    private static readonly Tour Sunset = TestData.Sunset(TestData.Monday);
    private static readonly Tour Morning = TestData.Morning(TestData.Monday);

    [TestMethod]
    public void Subtotal_AddsAdultAndChildFares()
    {
        Assert.AreEqual(188.00m, InvoiceService.Subtotal(Sunset, 3, 1));
        Assert.AreEqual(120.00m, InvoiceService.Subtotal(Morning, 2, 1));
    }

    [TestMethod]
    public void TaxOn_AppliesStateRate()
    {
        Assert.AreEqual(6.25m, InvoiceService.TaxOn(100.00m));
        Assert.AreEqual(11.75m, InvoiceService.TaxOn(188.00m));
        Assert.AreEqual(7.50m, InvoiceService.TaxOn(120.00m));
        Assert.AreEqual(0m, InvoiceService.TaxOn(0m));
    }

    [TestMethod]
    public void ForBooking_ListsFaresAndTotals()
    {
        var store = new InMemoryBookingStore([Sunset]);
        var customers = new Mock<ICustomerDirectory>();
        customers.Setup(c => c.Find(7)).Returns(new Customer(7, "kim@example.com", "Kim Ng", ""));
        var service = new InvoiceService(store, customers.Object);

        var invoice = service.ForBooking(new Booking(55, Sunset.Id, 7, 3, 1, DateTimeOffset.UnixEpoch));

        Assert.AreEqual("Kim Ng", invoice.CustomerName);
        Assert.AreEqual(2, invoice.Lines.Count);
        Assert.AreEqual(156.00m, invoice.Lines[0].Amount);
        Assert.AreEqual(32.00m, invoice.Lines[1].Amount);
        Assert.AreEqual(188.00m, invoice.Subtotal);
        Assert.AreEqual(11.75m, invoice.Tax);
        Assert.AreEqual(199.75m, invoice.Total);
    }
}
