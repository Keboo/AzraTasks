namespace AzraTasks.Core.Todos;

public interface ITodoListService
{
    IAsyncEnumerable<TodoListLite> GetListsAsync(CancellationToken cancellationToken);

    Task<TodoListFull?> GetListByIdAsync(Guid listId, CancellationToken cancellationToken);

    Task<TodoListLite> CreateListAsync(string name, CancellationToken cancellationToken);

    Task DeleteListAsync(Guid listId, CancellationToken cancellationToken);

    Task<TodoListItem> CreateItemAsync(Guid listId, string title, CancellationToken cancellationToken);

    Task<TodoListItem> UpdateItemAsync(Guid itemId, string title, CancellationToken cancellationToken);

    Task<TodoListItem> SetItemCompletedAsync(Guid itemId, bool isCompleted, CancellationToken cancellationToken);

    Task DeleteItemAsync(Guid itemId, CancellationToken cancellationToken);
}
