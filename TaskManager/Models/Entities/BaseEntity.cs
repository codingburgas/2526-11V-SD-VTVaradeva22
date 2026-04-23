namespace TaskManager.Models.Entities;

public abstract class BaseEntity
{
    // Primary key for all derived entities.
    public int Id { get; set; }

    // Save when the record was created.
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
