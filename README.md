# magrathea-whale

Scratch repo for Magrathea test runs. Contents are disposable; expect bot PRs.

## Whale tour bookings API

A small ASP.NET Core (.NET 10) API for a harbor whale-watching company: today's tours, bookings, cancellations and invoices. Data is in memory and seeded on start.

- Tours depart from the harbor (Eastern time): Morning Whale Watch at 10:00 (40 seats) and Harbor Sunset Cruise at 17:30 (20 seats), every day for the next week.
- Prices are per seat (adult or child); children must be booked with an adult. Massachusetts sales tax (6.25%) is added on the invoice.
- Cancellation policy: full refund 48 hours or more before departure, 50% between 24 and 48 hours, no refund within 24 hours.
- Customers sign in with their email address. Requests that act for a customer send their id in the `X-Customer-Id` header.

### Run it

.NET 10 SDK.

```sh
dotnet run --project src/Whale.Api        # http://localhost:5080
dotnet test
```

### Endpoints

| Method | Path | What |
|---|---|---|
| GET | `/health` | Liveness |
| GET | `/tours/today` | Today's departures with seats left |
| GET | `/tours/{id}` | One tour |
| POST | `/customers` | Sign in by email (creates the customer if new) |
| GET | `/customers/lookup?email=` | Find a customer by email |
| GET | `/bookings` | The caller's bookings |
| GET | `/bookings/{id}` | One booking |
| POST | `/bookings` | Book seats: `{ "tourId", "adults", "children" }` |
| POST | `/bookings/{id}/cancel` | Cancel; returns the refund |
| GET | `/bookings/{id}/invoice` | Invoice for a booking |

### Layout

| Path | What |
|---|---|
| `src/Whale.Api/Controllers` | HTTP endpoints |
| `src/Whale.Api/Services` | Booking, schedule, invoice, customer and payment logic |
| `src/Whale.Api/Data` | In-memory store and seed data |
| `tests/Whale.Api.Tests` | MSTest + Moq unit tests |
