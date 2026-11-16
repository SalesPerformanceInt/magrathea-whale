using Whale.Api.Data;
using Whale.Api.Models;
using Whale.Api.Services;

namespace Whale.Api.Tests;

[TestClass]
public sealed class CustomerDirectoryTests
{
    [TestMethod]
    public void FindOrCreate_ReturnsTheExistingCustomer()
    {
        var directory = new CustomerDirectory(SeedData.Customers);

        var raj = directory.FindOrCreate(new CustomerRequest("  raj.patel@example.com ", "Raj Patel", ""));

        Assert.AreEqual(2, raj.Id);
    }

    [TestMethod]
    public void FindOrCreate_CreatesNewCustomers()
    {
        var directory = new CustomerDirectory(SeedData.Customers);

        var kim = directory.FindOrCreate(new CustomerRequest("kim.ng@example.com", " Kim Ng ", "781-555-0100"));

        Assert.AreEqual(4, kim.Id);
        Assert.AreEqual("Kim Ng", kim.Name);
        Assert.AreSame(kim, directory.FindByEmail("kim.ng@example.com"));
        Assert.AreSame(kim, directory.Find(4));
    }

    [TestMethod]
    public void FindOrCreate_RequiresEmailAndName()
    {
        var directory = new CustomerDirectory([]);

        Assert.ThrowsException<BookingException>(() => directory.FindOrCreate(new CustomerRequest("not-an-email", "Kim", "")));
        Assert.ThrowsException<BookingException>(() => directory.FindOrCreate(new CustomerRequest("kim@example.com", " ", "")));
    }
}
