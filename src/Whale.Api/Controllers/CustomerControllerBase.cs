using Microsoft.AspNetCore.Mvc;

namespace Whale.Api.Controllers;

public abstract class CustomerControllerBase : ControllerBase
{
    public const string CustomerHeader = "X-Customer-Id";

    protected bool TryGetCaller(out int customerId)
    {
        customerId = 0;
        return Request.Headers.TryGetValue(CustomerHeader, out var value)
            && int.TryParse(value, out customerId)
            && customerId > 0;
    }
}
