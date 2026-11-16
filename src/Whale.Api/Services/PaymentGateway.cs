namespace Whale.Api.Services;

public interface IPaymentGateway
{
    Task<string> AuthorizeAsync(int customerId, decimal amount, CancellationToken cancellationToken = default);
}

public sealed class FakePaymentGateway : IPaymentGateway
{
    public async Task<string> AuthorizeAsync(int customerId, decimal amount, CancellationToken cancellationToken = default)
    {
        await Task.Delay(TimeSpan.FromMilliseconds(250), cancellationToken);
        return $"auth_{customerId}_{Guid.NewGuid():N}";
    }
}
