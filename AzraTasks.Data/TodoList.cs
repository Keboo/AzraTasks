using System.ComponentModel.DataAnnotations;

namespace AzraTasks.Data;

public class TodoList : UserObject
{
    [Required]
    [MaxLength(200)]
    public required string Name { get; set; }

    public ICollection<TodoItem> Items { get; set; } = [];
}
