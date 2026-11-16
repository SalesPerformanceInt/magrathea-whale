using Moq;
using Whale.Api.Data;
using Whale.Api.Models;
using Whale.Api.Services;

namespace Whale.Api.Tests;

[TestClass]
public sealed class BookingServiceTests
{
    private static readonly DateTimeOffset MondayMorning = Harbor.At(TestData.Monday, new TimeOnly(8, 0));

    private static (BookingService Service, InMemoryBookingStore Store, Mock<IPaymentGateway> Payments, FixedTime Time) Create(
        Tour tour, params Booking[] existing)
    {
        var store = new InMemoryBookingStore([tour], existing);
        var time = new FixedTime(MondayMorning);
        var payments = new Mock<IPaymentGateway>();
        payments.Setup(p => p.AuthorizeAsync(It.IsAny<int>(), It.IsAny<decimal>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("auth_test");
        var service = new BookingService(store, new TourSchedule(store, time), payments.Object, time);
        return (service, store, payments, time);
    }

    [TestMethod]
    public async Task BookAsync_ChargesAndSavesTheBooking()
    {
        var tour = TestData.Morning(TestData.Monday);
        var (service, store, payments, _) = Create(tour);

        var booking = await service.BookAsync(1, new BookingRequest(tour.Id, 2, 1));

        Assert.AreEqual(3, booking.Seats);
        Assert.AreSame(booking, store.FindBooking(booking.Id));
        payments.Verify(p => p.AuthorizeAsync(1, 127.50m, It.IsAny<CancellationToken>()), Times.Once);
    }

    [TestMethod]
    public async Task BookAsync_RefusesWhenNotEnoughSeats()
    {
        var tour = TestData.Sunset(TestData.Monday, capacity: 20);
        var (service, _, payments, _) = Create(tour, new Booking(1, tour.Id, 2, 19, 0, MondayMorning));

        var error = await Assert.ThrowsExceptionAsync<BookingException>(() => service.BookAsync(1, new BookingRequest(tour.Id, 2, 0)));

        StringAssert.Contains(error.Message, "Only 1 seats left");
        payments.VerifyNoOtherCalls();
    }

    [TestMethod]
    public async Task BookAsync_IgnoresCancelledSeats()
    {
        var tour = TestData.Sunset(TestData.Monday, capacity: 20);
        var cancelled = new Booking(1, tour.Id, 2, 19, 0, MondayMorning, BookingStatus.Cancelled);
        var (service, _, _, _) = Create(tour, cancelled);

        var booking = await service.BookAsync(1, new BookingRequest(tour.Id, 2, 0));

        Assert.AreEqual(2, booking.Seats);
    }

    [TestMethod]
    public async Task BookAsync_RefusesDepartedTours()
    {
        var tour = TestData.Morning(TestData.Monday);
        var (service, _, _, time) = Create(tour);
        time.Now = tour.Departure.AddMinutes(5);

        await Assert.ThrowsExceptionAsync<BookingException>(() => service.BookAsync(1, new BookingRequest(tour.Id, 1, 0)));
    }

    [TestMethod]
    public async Task BookAsync_ValidatesSeatCounts()
    {
        var tour = TestData.Morning(TestData.Monday);
        var (service, _, _, _) = Create(tour);

        await Assert.ThrowsExceptionAsync<BookingException>(() => service.BookAsync(1, new BookingRequest(tour.Id, 0, 0)));
        await Assert.ThrowsExceptionAsync<BookingException>(() => service.BookAsync(1, new BookingRequest(tour.Id, 0, 2)));
        await Assert.ThrowsExceptionAsync<BookingException>(() => service.BookAsync(1, new BookingRequest(tour.Id, -1, 3)));
        await Assert.ThrowsExceptionAsync<NotFoundException>(() => service.BookAsync(1, new BookingRequest(999, 1, 0)));
    }

    [TestMethod]
    public void Cancel_WellAheadRefundsInFull()
    {
        var tour = TestData.Sunset(TestData.Monday.AddDays(3));
        var (service, store, _, _) = Create(tour, new Booking(10, tour.Id, 1, 2, 1, MondayMorning));

        var result = service.Cancel(1, 10);

        Assert.AreEqual(InvoiceService.AmountDue(tour, 2, 1), result.RefundAmount);
        Assert.AreEqual(BookingStatus.Cancelled, store.FindBooking(10)!.Status);
    }

    [TestMethod]
    public void Cancel_SameDayRefundsNothing()
    {
        var tour = TestData.Morning(TestData.Monday);
        var (service, _, _, _) = Create(tour, new Booking(10, tour.Id, 1, 2, 0, MondayMorning));

        Assert.AreEqual(0m, service.Cancel(1, 10).RefundAmount);
    }

    [TestMethod]
    public void Cancel_OnlyTheOwnersBookingAndOnlyOnce()
    {
        var tour = TestData.Sunset(TestData.Monday.AddDays(3));
        var (service, _, _, _) = Create(tour, new Booking(10, tour.Id, 1, 2, 0, MondayMorning));

        Assert.ThrowsException<NotFoundException>(() => service.Cancel(2, 10));
        service.Cancel(1, 10);
        Assert.ThrowsException<BookingException>(() => service.Cancel(1, 10));
    }
}
