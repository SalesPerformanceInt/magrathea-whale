namespace Whale.Api.Services;

public sealed class BookingException(string message) : Exception(message);

public sealed class NotFoundException(string message) : Exception(message);
