namespace Whale.Api.Models;

public sealed record BookingRequest(int TourId, int Adults, int Children);

public sealed record CustomerRequest(string Email, string Name, string Phone);

public sealed record CancellationResult(int BookingId, decimal RefundAmount);
