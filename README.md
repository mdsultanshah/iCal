# Calendar prototype

Throwaway prototype of a single-page calendar. Fake data only. Appointments stay in this browser. This must not be merged as product work.

## Run

```bash
cd src/BookingCalendar.App
dotnet run
```

Then open `http://127.0.0.1:5088`.

- `/` is the only view: a Monday-start month that fills the screen. Click a day to add an appointment, or click an appointment to edit it.
- `/?reset=1` clears saved appointments and restores the three examples. It leaves the theme and the clock choice alone.
- `/?today=yyyy-mm-dd` fixes what "today" is. Any other value is ignored.
- `/?preview=empty` shows an empty calendar without saving that wipe.

Weeks start on Monday. The clock shows 24-hour times until you choose 12-hour, and that choice is remembered in this browser. The theme follows the system until you pick light or dark.

Headings use Cal Sans and text uses Inter. Both are self-hosted. If either file is missing, the page falls back to the system font stack.

Appointments are timed only. There is no all-day appointment. Week view is not in this build.

## Check

```bash
dotnet build -c Release -warnaserror
dotnet test -c Release
```
