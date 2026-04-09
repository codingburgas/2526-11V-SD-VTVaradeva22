using System.ComponentModel.DataAnnotations;

namespace TaskManager.Models.Entities;

public class Board : BaseEntity
{
    [Required]
    [StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [StringLength(250)]
    public string Description { get; set; } = string.Empty;

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public ApplicationUser Owner { get; set; } = null!;

    public ICollection<BoardList> Lists { get; set; } = new List<BoardList>();

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}
