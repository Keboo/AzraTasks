using AzraTasks.Data;

using Microsoft.EntityFrameworkCore;

namespace AzraTasks.Core.Todos;

public class TodoListService(ApplicationDbContext context) : ITodoListService
{
    public async Task<IEnumerable<TodoList>> GetListsAsync(CancellationToken cancellationToken)
    {
        return await context.TodoLists
            .OrderByDescending(x => x.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<TodoList?> GetListByIdAsync(Guid listId, CancellationToken cancellationToken)
    {
        return await context.TodoLists
            .Include(x => x.CreatedBy)
            .FirstOrDefaultAsync(x => x.Id == listId, cancellationToken);
    }

    public async Task<TodoList> CreateListAsync(string name, CancellationToken cancellationToken)
    {
        var normalizedName = NormalizeListName(name);

        var exists = await context.TodoLists
            .AnyAsync(
                list => EF.Functions.Like(list.Name, normalizedName),
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"You already have a list named '{normalizedName}'.");
        }

        var list = new TodoList
        {
            Name = normalizedName
        };

        context.TodoLists.Add(list);
        await context.SaveChangesAsync(cancellationToken);

        return list;
    }

    public async Task DeleteListAsync(Guid listId, CancellationToken cancellationToken)
    {
        var list = await context.TodoLists
            .Include(l => l.Items)
            .FirstOrDefaultAsync(l => l.Id == listId, cancellationToken)
            ?? throw new InvalidOperationException("Todo list not found.");

        context.TodoItems.RemoveRange(list.Items);
        context.TodoLists.Remove(list);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<TodoItem>> GetItemsAsync(Guid listId, CancellationToken cancellationToken)
    {
        return await context.TodoItems
            .Where(item => item.ListId == listId)
            .OrderByDescending(item => item.CreatedDate)
            .ToListAsync(cancellationToken);
    }

    public async Task<TodoItem> CreateItemAsync(Guid listId, string title, CancellationToken cancellationToken)
    {
        var normalizedTitle = NormalizeItemTitle(title);

        await EnsureOwnedListExistsAsync(context, listId, cancellationToken);

        var question = new TodoItem
        {
            ListId = listId,
            Text = normalizedTitle,
        };

        context.TodoItems.Add(question);
        await context.SaveChangesAsync(cancellationToken);

        return question;
    }

    public async Task<TodoItem> UpdateItemAsync(Guid listId, Guid itemId, string title, CancellationToken cancellationToken)
    {
        var normalizedTitle = NormalizeItemTitle(title);

        var item = await GetOwnedItemAsync(context, listId, itemId, cancellationToken);

        item.Text = normalizedTitle;
        item.LastModifiedDate = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<TodoItem> SetItemCompletedAsync(Guid listId, Guid itemId, bool isCompleted, CancellationToken cancellationToken)
    {
        var item = await GetOwnedItemAsync(context, listId, itemId, cancellationToken);

        item.IsComplete = isCompleted;
        item.LastModifiedDate = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task DeleteItemAsync(Guid listId, Guid itemId, CancellationToken cancellationToken)
    {
        var item = await GetOwnedItemAsync(context, listId, itemId, cancellationToken);

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
        CancellationToken cancellationToken)
    {
        var listExists = await context.TodoLists
            .Include(x => x.CreatedBy)
            .AnyAsync(x => x.Id == listId, cancellationToken);

        if (!listExists)
        {
            throw new InvalidOperationException("Todo list not found.");
        }
    }

    private static async Task<TodoItem> GetOwnedItemAsync(
        ApplicationDbContext context,
        Guid listId,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var item = await context.TodoItems
            .Include(item => item.List)
            .AsTracking()
            .FirstOrDefaultAsync(item => item.Id == itemId && item.ListId == listId, cancellationToken);

        return item ?? throw new InvalidOperationException("Todo item not found.");
    }
}
