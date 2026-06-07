using System.ComponentModel.DataAnnotations;

namespace AzraTasks.Data;

public class TodoList
{
    [Key]
    public Guid Id { get; set; }

    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    [Required]
    public required string CreatedById { get; set; }
    public ApplicationUser? CreatedBy { get; set; }

    public DateTimeOffset CreatedDate { get; set; } = DateTimeOffset.UtcNow;

    public ICollection<TodoItem> Items { get; set; } = [];
}
