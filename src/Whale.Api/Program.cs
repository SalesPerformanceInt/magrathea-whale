using Whale.Api.Data;
using Whale.Api.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddSingleton<IBookingStore>(sp => SeedData.CreateStore(sp.GetRequiredService<TimeProvider>()));
builder.Services.AddSingleton<ICustomerDirectory>(_ => new CustomerDirectory(SeedData.Customers));
builder.Services.AddSingleton<IPaymentGateway, FakePaymentGateway>();
builder.Services.AddSingleton<TourSchedule>();
builder.Services.AddSingleton<BookingService>();
builder.Services.AddSingleton<InvoiceService>();

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));
app.MapControllers();

app.Run();

public partial class Program;
