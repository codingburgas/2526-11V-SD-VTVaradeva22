using System.Collections.Generic;

namespace TaskManager.Models.Entities
{
    public class Board : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
    }
}