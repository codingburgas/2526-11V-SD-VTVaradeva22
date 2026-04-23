using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Helpers;

public static class Utilities
{
    public static DateTime StartOfWeekUtc(DateTime utcNow)
    {
        // Move back to Monday so weekly stats use a fixed start day.
        var date = utcNow.Date;
        var diff = (7 + (date.DayOfWeek - DayOfWeek.Monday)) % 7;
        return date.AddDays(-diff);
    }

    public static string GetStatusLabel(TaskItemStatus status)
    {
        // Convert enum values to friendly text for the UI.
        return status switch
        {
            TaskItemStatus.ToDo => "To Do",
            TaskItemStatus.InProgress => "In Progress",
            TaskItemStatus.Done => "Done",
            _ => status.ToString()
        };
    }
}
