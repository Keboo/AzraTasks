using AzraTasks.Data;

namespace AzraTasks.Core.Todos;

public interface ITodoListService
{
    Task<IEnumerable<TodoList>> GetListsAsync(CancellationToken cancellationToken);

    Task<TodoList?> GetListByIdAsync(Guid listId, CancellationToken cancellationToken);

    Task<TodoList> CreateListAsync(string name, CancellationToken cancellationToken);

    Task DeleteListAsync(Guid listId, CancellationToken cancellationToken);

    Task<IEnumerable<TodoItem>> GetItemsAsync(Guid listId, CancellationToken cancellationToken);

    Task<TodoItem> CreateItemAsync(Guid listId, string title, CancellationToken cancellationToken);

    Task<TodoItem> UpdateItemAsync(Guid listId, Guid itemId, string title, CancellationToken cancellationToken);

    Task<TodoItem> SetItemCompletedAsync(Guid listId, Guid itemId, bool isCompleted, CancellationToken cancellationToken);

    Task DeleteItemAsync(Guid listId, Guid itemId, CancellationToken cancellationToken);
}
