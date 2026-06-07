using AzraTasks.Data;

using Microsoft.EntityFrameworkCore;

namespace AzraTasks.Core.Todos;

public class TodoListService(ApplicationDbContext context) : ITodoListService
{
    public async Task<IEnumerable<TodoList>> GetListsAsync(string userId)
    {
        return await context.TodoLists
            .Include(x => x.CreatedBy)
            .Where(x => x.CreatedBy!.Id == userId)
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync();
    }

    public async Task<TodoList?> GetListByIdAsync(Guid listId, string userId)
    {
        return await context.TodoLists
            .Include(x => x.CreatedBy)
            .FirstOrDefaultAsync(x => x.Id == listId && x.CreatedBy!.Id == userId);
    }

    public async Task<TodoList> CreateListAsync(string name, string userId, CancellationToken cancellationToken)
    {
        var normalizedName = NormalizeListName(name);

        var exists = await context.TodoLists
            .Include(x => x.CreatedBy)
            .AnyAsync(
                room => room.CreatedBy!.Id == userId && EF.Functions.Like(room.Name, normalizedName),
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"You already have a list named '{normalizedName}'.");
        }

        var room = new TodoList
        {
            Id = Guid.NewGuid(),
            Name = normalizedName,
            CreatedById = userId,
            CreatedDate = DateTimeOffset.UtcNow
        };

        context.TodoLists.Add(room);
        await context.SaveChangesAsync(cancellationToken);

        return room;
    }

    public async Task DeleteListAsync(Guid listId, string userId, CancellationToken cancellationToken)
    {
        var room = await context.TodoLists
            .Include(existingRoom => existingRoom.Items)
            .Include(existingRoom => existingRoom.CreatedBy)
            .FirstOrDefaultAsync(
                existingRoom => existingRoom.Id == listId && existingRoom.CreatedBy!.Id == userId,
                cancellationToken)
            ?? throw new InvalidOperationException("Todo list not found.");

        context.TodoItems.RemoveRange(room.Items);
        context.TodoLists.Remove(room);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<TodoItem>> GetItemsAsync(Guid listId, string userId)
    {
        var listExists = await context.TodoLists
            .Include(x => x.CreatedBy)
            .AnyAsync(room => room.Id == listId && room.CreatedBy!.Id == userId);

        if (!listExists)
        {
            throw new InvalidOperationException("Todo list not found.");
        }

        return await context.TodoItems
            .Where(item => item.ListId == listId)
            .OrderByDescending(item => item.CreatedDate)
            .ToListAsync();
    }

    public async Task<TodoItem> CreateItemAsync(Guid listId, string title, string userId, CancellationToken cancellationToken)
    {
        var normalizedTitle = NormalizeItemTitle(title);

        await EnsureOwnedListExistsAsync(context, listId, userId, cancellationToken);

        var question = new TodoItem
        {
            Id = Guid.NewGuid(),
            ListId = listId,
            Text = normalizedTitle,
            CreatedDate = DateTimeOffset.UtcNow
        };

        context.TodoItems.Add(question);
        await context.SaveChangesAsync(cancellationToken);

        return question;
    }

    public async Task<TodoItem> UpdateItemAsync(Guid listId, Guid itemId, string title, string userId, CancellationToken cancellationToken)
    {
        var normalizedTitle = NormalizeItemTitle(title);

        var item = await GetOwnedItemAsync(context, listId, itemId, userId, cancellationToken);

        item.Text = normalizedTitle;
        item.LastModifiedDate = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<TodoItem> SetItemCompletedAsync(Guid listId, Guid itemId, bool isCompleted, string userId, CancellationToken cancellationToken)
    {
        var item = await GetOwnedItemAsync(context, listId, itemId, userId, cancellationToken);

        item.IsComplete = isCompleted;
        item.LastModifiedDate = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task DeleteItemAsync(Guid listId, Guid itemId, string userId, CancellationToken cancellationToken)
    {
        var item = await GetOwnedItemAsync(context, listId, itemId, userId, cancellationToken);

        context.TodoItems.Remove(item);
        await context.SaveChangesAsync(cancellationToken);
    }

    private static string NormalizeListName(string name)
    {
        var normalized = name.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Todo list name is required.");
        }

        if (normalized.Length > 200)
        {
            throw new InvalidOperationException("Todo list name must be 200 characters or fewer.");
        }

        return normalized;
    }

    private static string NormalizeItemTitle(string title)
    {
        var normalized = title.Trim();

        if (string.IsNullOrWhiteSpace(normalized))
        {
            throw new InvalidOperationException("Todo item title is required.");
        }

        if (normalized.Length > 2000)
        {
            throw new InvalidOperationException("Todo item title must be 2000 characters or fewer.");
        }

        return normalized;
    }

    private static async Task EnsureOwnedListExistsAsync(
        ApplicationDbContext context,
        Guid listId,
        string userId,
        CancellationToken cancellationToken)
    {
        var listExists = await context.TodoLists
            .Include(x => x.CreatedBy)
            .AnyAsync(x => x.Id == listId && x.CreatedBy!.Id == userId, cancellationToken);

        if (!listExists)
        {
            throw new InvalidOperationException("Todo list not found.");
        }
    }

    private static async Task<TodoItem> GetOwnedItemAsync(
        ApplicationDbContext context,
        Guid listId,
        Guid itemId,
        string userId,
        CancellationToken cancellationToken)
    {
        var item = await context.TodoItems
            .Include(item => item.List)
            .AsTracking()
            .FirstOrDefaultAsync(
                item => item.Id == itemId
                    && item.ListId == listId
                    && item.List!.CreatedBy!.Id == userId,
                cancellationToken);

        return item ?? throw new InvalidOperationException("Todo item not found.");
    }
}
