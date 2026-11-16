namespace Whale.Api.Models;

public sealed record InvoiceLine(string Description, int Quantity, decimal UnitPrice, decimal Amount);

public sealed record Invoice(int BookingId, string CustomerName, IReadOnlyList<InvoiceLine> Lines, decimal Subtotal, decimal Tax, decimal Total);
