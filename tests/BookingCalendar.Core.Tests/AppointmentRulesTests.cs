using BookingCalendar.Core;

namespace BookingCalendar.Core.Tests;

public class AppointmentRulesTests
{
    private static readonly DateOnly Day = new(2026, 10, 21);

    [Fact]
    public void Title_is_required()
    {
        Assert.Equal("Enter a title.", AppointmentRules.TitleError(null));
        Assert.Equal("Enter a title.", AppointmentRules.TitleError("   "));
        Assert.Null(AppointmentRules.TitleError("Colour plan"));
    }

    [Fact]
    public void End_must_be_after_the_start()
    {
        Assert.Equal("End must be after the start.", AppointmentRules.TimeOrderError(new TimeOnly(10, 0), new TimeOnly(10, 0)));
        Assert.Equal("End must be after the start.", AppointmentRules.TimeOrderError(new TimeOnly(11, 0), new TimeOnly(10, 0)));
        Assert.Null(AppointmentRules.TimeOrderError(new TimeOnly(10, 0), new TimeOnly(10, 30)));
    }

    [Fact]
    public void Save_adds_a_trimmed_appointment_and_keeps_the_rest()
    {
        var existing = ExampleAppointments.Create(Day);
        var outcome = AppointmentRules.Save(
            existing,
            null,
            "  Front desk  ",
            Day,
            new TimeOnly(15, 0),
            new TimeOnly(16, 0),
            "  Bring the key  ",
            () => "new-1");

        Assert.True(outcome.Ok);
        Assert.Equal(4, outcome.Appointments.Count);
        var added = Assert.Single(outcome.Appointments, item => item.Id == "new-1");
        Assert.Equal("Front desk", added.Title);
        Assert.Equal("Bring the key", added.Note);
        Assert.False(added.IsExample);
        Assert.Equal(3, outcome.Appointments.Count(item => item.IsExample));
    }

    [Fact]
    public void Save_rejects_a_missing_title_and_a_reversed_time_without_changing_the_list()
    {
        var existing = ExampleAppointments.Create(Day);
        var missing = AppointmentRules.Save(existing, null, " ", Day, new TimeOnly(9, 0), new TimeOnly(10, 0), "", () => "x");
        Assert.Equal("Enter a title.", missing.TitleError);
        Assert.Null(missing.TimeError);
        Assert.Same(existing, missing.Appointments);

        var reversed = AppointmentRules.Save(existing, null, "Front desk", Day, new TimeOnly(16, 0), new TimeOnly(15, 0), "", () => "x");
        Assert.Null(reversed.TitleError);
        Assert.Equal("End must be after the start.", reversed.TimeError);
        Assert.Equal(3, reversed.Appointments.Count);
    }

    [Fact]
    public void Save_edits_an_example_in_place_and_keeps_the_example_flag()
    {
        var existing = ExampleAppointments.Create(Day);
        var outcome = AppointmentRules.Save(
            existing,
            "ex-colour",
            "Colour plan updated",
            Day,
            new TimeOnly(10, 0),
            new TimeOnly(12, 0),
            "",
            () => "should-not-be-used");

        Assert.True(outcome.Ok);
        Assert.Equal(3, outcome.Appointments.Count);
        var edited = Assert.Single(outcome.Appointments, item => item.Id == "ex-colour");
        Assert.Equal("Colour plan updated", edited.Title);
        Assert.Equal(new TimeOnly(12, 0), edited.End);
        Assert.True(edited.IsExample);
    }

    [Fact]
    public void Delete_removes_one_appointment()
    {
        var existing = ExampleAppointments.Create(Day);
        var left = AppointmentRules.Delete(existing, "ex-trim");
        Assert.Equal(2, left.Count);
        Assert.DoesNotContain(left, item => item.Id == "ex-trim");
        Assert.Equal(existing.Count, AppointmentRules.Delete(existing, "missing").Count);
    }

    [Fact]
    public void Clear_examples_removes_only_the_samples()
    {
        var existing = ExampleAppointments.Create(Day);
        var saved = AppointmentRules.Save(existing, null, "Mine", Day, new TimeOnly(8, 0), new TimeOnly(9, 0), "", () => "mine");
        var left = AppointmentRules.ClearExamples(saved.Appointments);
        var only = Assert.Single(left);
        Assert.Equal("mine", only.Id);
        Assert.False(only.IsExample);
    }

    [Fact]
    public void Examples_are_exactly_three_and_two_of_them_overlap()
    {
        var seeded = ExampleAppointments.Create(new DateOnly(2026, 10, 21));
        Assert.Equal(3, seeded.Count);
        Assert.All(seeded, item => Assert.True(item.IsExample));
        Assert.All(seeded, item => Assert.True(CalendarRange.InWeek(item.Date, new DateOnly(2026, 10, 21))));

        var overlapping = seeded.Where(item => item.Date == seeded[0].Date).ToList();
        Assert.Equal(2, overlapping.Count);
        Assert.True(OverlapLayout.Overlaps(overlapping[0], overlapping[1]));
        Assert.False(OverlapLayout.Overlaps(seeded[0], seeded[2]));
    }
}

public class OverlapLayoutTests
{
    private static Appointment At(string id, int startHour, int startMinute, int endHour, int endMinute) =>
        new(id, id, new DateOnly(2026, 10, 21), new TimeOnly(startHour, startMinute), new TimeOnly(endHour, endMinute), "", false);

    [Fact]
    public void Overlapping_appointments_sit_in_different_columns()
    {
        var placed = OverlapLayout.Place(
        [
            At("a", 10, 0, 11, 0),
            At("b", 10, 30, 11, 30),
        ]);

        Assert.Equal(2, placed.Count);
        Assert.All(placed, item => Assert.Equal(2, item.ColumnCount));
        Assert.Equal([0, 1], placed.Select(item => item.Column).OrderBy(column => column).ToArray());
    }

    [Fact]
    public void Separate_appointments_each_use_the_full_width()
    {
        var placed = OverlapLayout.Place(
        [
            At("morning", 9, 0, 10, 0),
            At("afternoon", 14, 0, 15, 0),
        ]);

        Assert.All(placed, item => Assert.Equal(0, item.Column));
        Assert.All(placed, item => Assert.Equal(1, item.ColumnCount));
    }

    [Fact]
    public void Touching_endpoints_do_not_overlap()
    {
        var first = At("a", 10, 0, 11, 0);
        var second = At("b", 11, 0, 12, 0);
        Assert.False(OverlapLayout.Overlaps(first, second));

        var placed = OverlapLayout.Place([first, second]);
        Assert.All(placed, item => Assert.Equal(1, item.ColumnCount));
    }

    [Fact]
    public void A_chain_reuses_a_free_column()
    {
        var placed = OverlapLayout.Place(
        [
            At("a", 9, 0, 11, 0),
            At("b", 10, 0, 12, 0),
            At("c", 11, 30, 13, 0),
        ]);

        Assert.Equal(2, placed.Max(item => item.ColumnCount));
        Assert.Equal(0, placed.Single(item => item.Appointment.Id == "a").Column);
        Assert.Equal(1, placed.Single(item => item.Appointment.Id == "b").Column);
        Assert.Equal(0, placed.Single(item => item.Appointment.Id == "c").Column);
    }

    [Fact]
    public void Different_days_are_placed_on_their_own()
    {
        var otherDay = At("b", 10, 0, 11, 0) with { Date = new DateOnly(2026, 10, 22) };
        var placed = OverlapLayout.Place([At("a", 10, 0, 11, 0), otherDay]);
        Assert.All(placed, item => Assert.Equal(1, item.ColumnCount));
    }
}

public class CalendarRangeTests
{
    [Fact]
    public void Week_containing_wednesday_21_october_2026_starts_on_sunday()
    {
        var today = new DateOnly(2026, 10, 21);
        var week = CalendarRange.Week(today);
        Assert.Equal(new DateOnly(2026, 10, 18), week[0]);
        Assert.Equal(new DateOnly(2026, 10, 24), week[6]);
        Assert.Equal(7, week.Count);
        Assert.Contains(today, week);
        Assert.Equal(new DateOnly(2026, 10, 25), CalendarRange.Shift(week[0], 7));
    }

    [Fact]
    public void A_clicked_hour_ends_an_hour_later_except_the_last_hour()
    {
        Assert.Equal(new TimeOnly(10, 0), CalendarRange.DefaultEnd(new TimeOnly(9, 0)));
        Assert.Equal(new TimeOnly(23, 59), CalendarRange.DefaultEnd(new TimeOnly(23, 0)));
        Assert.True(CalendarRange.DefaultEnd(new TimeOnly(23, 0)) > new TimeOnly(23, 0));
    }

    [Fact]
    public void Preview_and_view_options_parse()
    {
        var options = PrototypeOptions.Parse("http://local/?reset=1&today=2026-10-21&preview=empty&view=day");
        Assert.True(options.Reset);
        Assert.True(options.PreviewEmpty);
        Assert.Equal(new DateOnly(2026, 10, 21), options.Today);
        Assert.Equal("day", options.View);
    }
}
