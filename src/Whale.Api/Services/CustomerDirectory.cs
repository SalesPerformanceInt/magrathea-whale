using Whale.Api.Models;

namespace Whale.Api.Services;

public interface ICustomerDirectory
{
    Customer? Find(int customerId);
    Customer? FindByEmail(string email);
    Customer FindOrCreate(CustomerRequest request);
}

public sealed class CustomerDirectory : ICustomerDirectory
{
    private readonly Dictionary<int, Customer> _byId = new();
    private readonly Dictionary<string, Customer> _byEmail = new();
    private readonly Lock _gate = new();
    private int _nextId;

    public CustomerDirectory(IEnumerable<Customer> customers)
    {
        foreach (var customer in customers)
        {
            _byId[customer.Id] = customer;
            _byEmail[customer.Email.Trim()] = customer;
            _nextId = Math.Max(_nextId, customer.Id);
        }
    }

    public Customer? Find(int customerId)
    {
        lock (_gate)
        {
            return _byId.GetValueOrDefault(customerId);
        }
    }

    public Customer? FindByEmail(string email)
    {
        lock (_gate)
        {
            return _byEmail.GetValueOrDefault(email.Trim());
        }
    }

    public Customer FindOrCreate(CustomerRequest request)
    {
        var email = request.Email?.Trim() ?? "";
        if (email.Length < 3 || !email.Contains('@'))
        {
            throw new BookingException("A valid email address is required.");
        }
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BookingException("A name is required.");
        }
        lock (_gate)
        {
            if (_byEmail.TryGetValue(email, out var existing))
            {
                return existing;
            }
            var customer = new Customer(++_nextId, email, request.Name.Trim(), request.Phone?.Trim() ?? "");
            _byId[customer.Id] = customer;
            _byEmail[email] = customer;
            return customer;
        }
    }
}
