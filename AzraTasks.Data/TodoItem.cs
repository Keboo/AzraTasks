using System.ComponentModel.DataAnnotations;

namespace AzraTasks.Data;

public class TodoItem
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    public required Guid ListId { get; set; }
    public TodoList? List { get; set; }

    [Required]
    [MaxLength(2000)]
    public required string Text { get; set; }

    public bool IsComplete { get; set; }

    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? LastModifiedDate { get; set; }
}