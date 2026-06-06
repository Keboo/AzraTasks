using AzraTasks.Data;

namespace AzraTasks.Core.Todos;

public interface ITodoListService
{
    Task<IEnumerable<Room>> GetListsAsync(string userId);

    Task<Room?> GetListByIdAsync(Guid listId, string userId);

    Task<Room> CreateListAsync(string name, string userId, CancellationToken cancellationToken);

    Task DeleteListAsync(Guid listId, string userId, CancellationToken cancellationToken);

    Task<IEnumerable<Question>> GetItemsAsync(Guid listId, string userId);

    Task<Question> CreateItemAsync(Guid listId, string title, string userId, CancellationToken cancellationToken);

    Task<Question> UpdateItemAsync(Guid listId, Guid itemId, string title, string userId, CancellationToken cancellationToken);

    Task<Question> SetItemCompletedAsync(Guid listId, Guid itemId, bool isCompleted, string userId, CancellationToken cancellationToken);

    Task DeleteItemAsync(Guid listId, Guid itemId, string userId, CancellationToken cancellationToken);
}
