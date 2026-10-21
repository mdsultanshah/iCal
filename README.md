# Booking Calendar prototype

Throwaway prototype of a phone-first booking calendar for **North Quay Studio**. Fake data only. Bookings stay in this browser. This must not be merged as product work.

## Run

```bash
cd src/BookingCalendar.App
dotnet run
```

Then open `http://127.0.0.1:5088`.

- `/setup` sets weekly hours, chooses 1 to 3 services, and shows one share link.
- `/` is the customer booking page.
- `/?reset=1` restores the seeded bookings and the default hours.
- `/?today=yyyy-mm-dd` fixes what "today" is.
- `/times?preview=empty` opens a day with no times (for screenshots). Pair it with `reset=1` and `today=yyyy-mm-dd`.

Times are shown in the browser's time zone. The business hours are Asia/Singapore (UTC+8) unless the owner changes the weekly hours.

## Check

```bash
dotnet build -c Release -warnaserror
dotnet test -c Release
```
