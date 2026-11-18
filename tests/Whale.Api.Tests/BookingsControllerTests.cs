using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Whale.Api.Controllers;
using Whale.Api.Data;
using Whale.Api.Models;
using Whale.Api.Services;

namespace Whale.Api.Tests;

[TestClass]
public sealed class BookingsControllerTests
{
    private static readonly DateTimeOffset MondayMorning = Harbor.At(TestData.Monday, new TimeOnly(8, 0));

    private static BookingsController CreateController(IBookingStore store, ICustomerDirectory customers, int callerId)
    {
        var time = new FixedTime(MondayMorning);
        var payments = new Mock<IPaymentGateway>();
        var service = new BookingService(store, new TourSchedule(store, time), payments.Object, time);
        var invoices = new InvoiceService(store, customers);
        var controller = new BookingsController(store, customers, service, invoices)
        {
            ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() },
        };
        controller.Request.Headers[CustomerControllerBase.CustomerHeader] = callerId.ToString();
        return controller;
    }

    [TestMethod]
    public void TEST1_14_Get_DoesNotRevealAnotherCustomersBooking()
    {
        // Booking 10 belongs to customer 1. Customer 2 requests it by changing
        // the booking number in the URL and must not see it.
        var tour = TestData.Morning(TestData.Monday);
        var owner = new Customer(1, "owner@example.com", "Test Owner", "555-0100");
        var stranger = new Customer(2, "stranger@example.com", "Other User", "555-0199");
        var store = new InMemoryBookingStore([tour], [new Booking(10, tour.Id, owner.Id, 2, 0, MondayMorning)]);
        var customers = new CustomerDirectory([owner, stranger]);

        var controller = CreateController(store, customers, callerId: stranger.Id);

        var response = controller.Get(10);

        Assert.IsNull(response.Value, "Another customer's booking details must not be returned.");
        Assert.IsInstanceOfType<NotFoundResult>(response.Result,
            "Requesting someone else's booking should respond 404 Not Found.");
    }
}
