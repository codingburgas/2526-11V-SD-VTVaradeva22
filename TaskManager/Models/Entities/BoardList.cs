using System.ComponentModel.DataAnnotations;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Models.Entities;

public class BoardList : BaseEntity
{
    [Required]
    [StringLength(80)]
    public string Title { get; set; } = string.Empty;

    public TaskItemStatus Status { get; set; }

    public int Position { get; set; }

    public int BoardId { get; set; }

    public Board Board { get; set; } = null!;

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
