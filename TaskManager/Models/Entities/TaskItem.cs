using System.ComponentModel.DataAnnotations;
using TaskManager.Models.Enums;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Models.Entities;

public class TaskItem : BaseEntity
{
    // Task title shown in cards and tables.
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    // Extra task details.
    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    // Optional deadline for the task.
    public DateTime? Deadline { get; set; }

    // How important the task is.
    public Priority Priority { get; set; }

    // Current workflow status.
    public TaskItemStatus Status { get; set; }

    // Keeps the task order inside the list.
    public int Position { get; set; }

    // Filled when the task is completed.
    public DateTime? CompletedAt { get; set; }

    public int BoardId { get; set; }

    public Board Board { get; set; } = null!;

    public int BoardListId { get; set; }

    public BoardList BoardList { get; set; } = null!;

    // The user assigned to work on the task.
    public string? AssignedToId { get; set; }

    public ApplicationUser? AssignedTo { get; set; }
}
