namespace Whale.Api.Models;

public sealed record Tour(int Id, string Name, DateTimeOffset Departure, int Capacity, decimal AdultPrice, decimal ChildPrice);
