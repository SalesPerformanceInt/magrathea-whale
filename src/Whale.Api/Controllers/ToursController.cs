using Microsoft.AspNetCore.Mvc;
using Whale.Api.Data;
using Whale.Api.Services;

namespace Whale.Api.Controllers;

[ApiController]
[Route("tours")]
public sealed class ToursController(IBookingStore store, TourSchedule schedule) : ControllerBase
{
    [HttpGet("today")]
    public IReadOnlyList<TourAvailability> Today() => schedule.Today();

    [HttpGet("{id:int}")]
    public ActionResult<TourAvailability> Get(int id)
    {
        var tour = store.FindTour(id);
        return tour is null ? NotFound() : schedule.Availability(tour);
    }
}
