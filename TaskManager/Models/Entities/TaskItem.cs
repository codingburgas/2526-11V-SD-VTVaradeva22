using System.ComponentModel.DataAnnotations;
using TaskManager.Models.Enums;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Models.Entities;

public class TaskItem : BaseEntity
{
    [Required]
    [StringLength(100)]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000)]
    public string Description { get; set; } = string.Empty;

    public DateTime? Deadline { get; set; }

    public Priority Priority { get; set; }

    public TaskItemStatus Status { get; set; }

    public int Position { get; set; }

    public DateTime? CompletedAt { get; set; }

    public int BoardId { get; set; }

    public Board Board { get; set; } = null!;

    public int BoardListId { get; set; }

    public BoardList BoardList { get; set; } = null!;

    public string? AssignedToId { get; set; }

    public ApplicationUser? AssignedTo { get; set; }
}
