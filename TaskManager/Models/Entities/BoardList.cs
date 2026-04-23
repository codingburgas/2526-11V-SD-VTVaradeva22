using System.ComponentModel.DataAnnotations;
using TaskItemStatus = TaskManager.Models.Enums.TaskStatus;

namespace TaskManager.Models.Entities;

public class BoardList : BaseEntity
{
    // Column title shown on the board page.
    [Required]
    [StringLength(80)]
    public string Title { get; set; } = string.Empty;

    // The status this list gives to its tasks.
    public TaskItemStatus Status { get; set; }

    // Keeps the visual order of lists.
    public int Position { get; set; }

    public int BoardId { get; set; }

    public Board Board { get; set; } = null!;

    // Tasks currently inside this list.
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
