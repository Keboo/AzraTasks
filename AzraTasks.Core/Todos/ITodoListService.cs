using AzraTasks.Data;

namespace AzraTasks.Core.Todos;

public interface ITodoListService
{
    Task<IEnumerable<TodoList>> GetListsAsync(string userId);

    Task<TodoList?> GetListByIdAsync(Guid listId, string userId);

    Task<TodoList> CreateListAsync(string name, string userId, CancellationToken cancellationToken);

    Task DeleteListAsync(Guid listId, string userId, CancellationToken cancellationToken);

    Task<IEnumerable<TodoItem>> GetItemsAsync(Guid listId, string userId);

    Task<TodoItem> CreateItemAsync(Guid listId, string title, string userId, CancellationToken cancellationToken);

    Task<TodoItem> UpdateItemAsync(Guid listId, Guid itemId, string title, string userId, CancellationToken cancellationToken);

    Task<TodoItem> SetItemCompletedAsync(Guid listId, Guid itemId, bool isCompleted, string userId, CancellationToken cancellationToken);

    Task DeleteItemAsync(Guid listId, Guid itemId, string userId, CancellationToken cancellationToken);
}
