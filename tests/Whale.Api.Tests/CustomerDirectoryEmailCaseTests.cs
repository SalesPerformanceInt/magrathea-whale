using Whale.Api.Models;
using Whale.Api.Services;

namespace Whale.Api.Tests;

// TEST1-13: a customer who signed up with a mixed-case email cannot be found
// when she signs in or is looked up with the same email in lowercase, so a
// duplicate account is created and her bookings page is empty.
[TestClass]
public sealed class CustomerDirectoryEmailCaseTests
{
    private static CustomerDirectory Directory() =>
        new([new Customer(1, "Test.User@example.com", "Test User", "555-0100")]);

    [TestMethod]
    public void TEST1_13_SignInWithLowercaseEmail_ReturnsTheExistingCustomer()
    {
        var directory = Directory();

        var customer = directory.FindOrCreate(new CustomerRequest("test.user@example.com", "Test User", ""));

        Assert.AreEqual(1, customer.Id, "Signing in with the same email in lowercase must return the existing account, not create a duplicate.");
    }

    [TestMethod]
    public void TEST1_13_FindByEmail_IsCaseInsensitive()
    {
        var directory = Directory();

        var customer = directory.FindByEmail("test.user@example.com");

        Assert.IsNotNull(customer, "Looking up an email must not depend on letter case.");
        Assert.AreEqual(1, customer.Id);
    }
}
