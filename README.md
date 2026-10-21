# Booking Calendar prototype

Throwaway clickable prototype of a phone-first booking calendar for **North Quay Studio** (one provider, three services). Fake data only. Bookings stay in the browser. This is not for production and must not be merged as product work.

## Run

```bash
cd src/BookingCalendar.App
dotnet run
```

Then open `http://127.0.0.1:5088`.

Test-only URL options:

- `/?reset=1` clears browser data back to the seeded bookings.
- `/?today=2026-10-21` fixes what "today" is (`yyyy-mm-dd`).

Slots are calculated in Singapore time (Asia/Singapore, UTC+8), Monday to Friday, 9:00–17:00.

## Check

```bash
dotnet build -c Release -warnaserror
dotnet test -c Release
```

The published site is static (Blazor WebAssembly). There is no backend and no external API.
