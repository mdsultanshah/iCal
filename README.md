# Appointments prototype

Throwaway prototype of a single-page calendar for **North Quay Studio**. Fake data only. Appointments stay in this browser. This must not be merged as product work.

## Run

```bash
cd src/BookingCalendar.App
dotnet run
```

Then open `http://127.0.0.1:5088`.

- `/` is the calendar. It shows a week, and a single day when the window is narrow.
- `/?reset=1` clears saved appointments and restores the three examples.
- `/?today=yyyy-mm-dd` fixes what "today" is.
- `/?preview=empty` shows an empty calendar without saving that wipe.
- `/?view=day` or `/?view=week` forces the layout.

Times are shown in the browser's time zone. That zone is named once in the header.

## Check

```bash
dotnet build -c Release -warnaserror
dotnet test -c Release
```
