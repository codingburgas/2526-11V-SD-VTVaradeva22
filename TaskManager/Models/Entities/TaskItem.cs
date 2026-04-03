using System;
using TaskManager.Models.Enums;
using TaskStatus = System.Threading.Tasks.TaskStatus;

namespace TaskManager.Models.Entities
{
    public class TaskItem : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public DateTime? Deadline { get; set; }
        public Priority Priority { get; set; }
        public TaskStatus Status { get; set; }

        public int BoardId { get; set; }
        public Board Board { get; set; }
    }
}