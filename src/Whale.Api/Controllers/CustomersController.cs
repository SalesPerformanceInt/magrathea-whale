using Microsoft.AspNetCore.Mvc;
using Whale.Api.Data;
using Whale.Api.Models;
using Whale.Api.Services;

namespace Whale.Api.Controllers;

public sealed record CustomerSummary(int Id, string Name, string Email, int Bookings);

[ApiController]
[Route("customers")]
public sealed class CustomersController(ICustomerDirectory customers, IBookingStore store) : ControllerBase
{
    [HttpPost]
    public ActionResult<CustomerSummary> SignIn(CustomerRequest request)
    {
        try
        {
            return Summary(customers.FindOrCreate(request));
        }
        catch (BookingException e)
        {
            return BadRequest(new { error = e.Message });
        }
    }

    [HttpGet("lookup")]
    public ActionResult<CustomerSummary> Lookup([FromQuery] string email)
    {
        var customer = customers.FindByEmail(email);
        return customer is null ? NotFound() : Summary(customer);
    }

    private CustomerSummary Summary(Customer customer) =>
        new(customer.Id, customer.Name, customer.Email, store.BookingsForCustomer(customer.Id).Count);
}
