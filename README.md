# Appointments prototype

Throwaway prototype of a single-page calendar for **North Quay Studio**. Fake data only. Appointments stay in this browser. This must not be merged as product work.

## Run

```bash
cd src/BookingCalendar.App
dotnet run
```

Then open `http://127.0.0.1:5088`.

- `/` opens the month. Week and day are the other two views. On a narrow window the three columns stack.
- `/?reset=1` clears saved appointments and restores the three examples. It leaves the theme and the clock choice alone.
- `/?today=yyyy-mm-dd` fixes what "today" is. Any other value is ignored.
- `/?preview=empty` shows an empty calendar without saving that wipe.
- `/?view=month`, `/?view=week`, or `/?view=day` opens that view. Anything else is ignored.

Weeks start on Monday. The clock shows 24-hour times until you choose 12-hour, and that choice is remembered in this browser. The theme follows the system until you pick light or dark.

Times use the browser's time zone. That zone is named once, in the details column.

Headings use Cal Sans and text uses Inter. Both are self-hosted. If either file is missing, the page falls back to the system font stack.

Appointments are timed only. There is no all-day appointment.

## Check

```bash
dotnet build -c Release -warnaserror
dotnet test -c Release
```
