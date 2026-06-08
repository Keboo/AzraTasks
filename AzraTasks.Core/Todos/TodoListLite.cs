namespace AzraTasks.Core.Todos;

public record class TodoListLite(Guid Id, string Name, int ItemCount, int CompletedItemCount, DateTimeOffset LastModified);


public record class TodoListFull(Guid Id, string Name, DateTimeOffset LastModified, IReadOnlyList<TodoListItem> Items);


public record class TodoListItem(Guid Id, string Text, bool IsComplete, DateTimeOffset LastModified)
{
    public static TodoListItem FromEntity(Data.TodoItem entity) => new(
        entity.Id,
        entity.Text,
        entity.IsComplete,
        entity.LastModifiedDate);
}