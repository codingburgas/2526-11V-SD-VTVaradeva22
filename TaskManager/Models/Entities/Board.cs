using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models.Entities;

public class Board : BaseEntity
{
    // Main board title.
    [Required]
    [StringLength(80)]
    public string Name { get; set; } = string.Empty;

    // Short board description.
    [StringLength(250)]
    public string Description { get; set; } = string.Empty;

    // The user who owns this board.
    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public ApplicationUser Owner { get; set; } = null!;

    // Lists shown inside the board.
    public ICollection<BoardList> Lists { get; set; } = new List<BoardList>();

    // All tasks linked to this board.
    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
