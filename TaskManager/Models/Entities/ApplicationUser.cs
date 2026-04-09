using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace TaskManager.Models.Entities;

public class ApplicationUser : IdentityUser
{
    [Required]
    [StringLength(100)]
    public string FullName { get; set; } = string.Empty;

    public ICollection<Board> Boards { get; set; } = new List<Board>();

    public ICollection<TaskItem> AssignedTasks { get; set; } = new List<TaskItem>();
}
