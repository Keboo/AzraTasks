using System.Runtime.CompilerServices;

using AzraTasks.Data;

using Microsoft.EntityFrameworkCore;

namespace AzraTasks.Core.Todos;

public class TodoListService(ApplicationDbContext context) : ITodoListService
{
    public async IAsyncEnumerable<TodoListLite> GetListsAsync([EnumeratorCancellation] CancellationToken cancellationToken)
    {
        await foreach (var entity in context.TodoLists
            .Include(x => x.Items)
            .OrderByDescending(x => x.CreatedDate)
            .Select(x => new
            {
                x.Id,
                x.Name,
                ItemCount = x.Items.Count,
                CompletedItemCount = x.Items.Count(x => x.IsComplete),
                x.LastModifiedDate
            })
            .AsAsyncEnumerable()
            .WithCancellation(cancellationToken))
        {
            yield return new TodoListLite(entity.Id, entity.Name, entity.ItemCount, entity.CompletedItemCount, entity.LastModifiedDate);
        }
    }

    public async Task<TodoListFull?> GetListByIdAsync(Guid listId, CancellationToken cancellationToken)
    {
        var list = await context.TodoLists
            .Include(x => x.Items)
            .FirstOrDefaultAsync(x => x.Id == listId, cancellationToken);

        if (list is null) return null;

        return new TodoListFull(list.Id, list.Name, list.LastModifiedDate,
            [.. list.Items.Select(TodoListItem.FromEntity)]);
    }

    public async Task<TodoListLite> CreateListAsync(string name, CancellationToken cancellationToken)
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

        return new TodoListLite(list.Id, list.Name, 0, 0, list.LastModifiedDate);
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

    public async Task<TodoListItem> CreateItemAsync(Guid listId, string title, CancellationToken cancellationToken)
    {
        var listExists = await context.TodoLists
            .Include(x => x.CreatedBy)
            .AnyAsync(x => x.Id == listId, cancellationToken);

        if (!listExists)
        {
            throw new InvalidOperationException("Todo list not found.");
        }

        var normalizedTitle = NormalizeItemTitle(title);

        var item = new TodoItem
        {
            ListId = listId,
            Text = normalizedTitle,
        };

        context.TodoItems.Add(item);
        await context.SaveChangesAsync(cancellationToken);

        return TodoListItem.FromEntity(item);
    }

    public async Task<TodoListItem> UpdateItemAsync(Guid itemId, string title, CancellationToken cancellationToken)
    {
        var normalizedTitle = NormalizeItemTitle(title);

        var item = await GetOwnedItemAsync(context, itemId, cancellationToken);

        item.Text = normalizedTitle;

        await context.SaveChangesAsync(cancellationToken);
        return TodoListItem.FromEntity(item);
    }

    public async Task<TodoListItem> SetItemCompletedAsync(Guid itemId, bool isCompleted, CancellationToken cancellationToken)
    {
        var item = await GetOwnedItemAsync(context, itemId, cancellationToken);

        item.IsComplete = isCompleted;

        await context.SaveChangesAsync(cancellationToken);
        return TodoListItem.FromEntity(item);
    }

    public async Task DeleteItemAsync(Guid itemId, CancellationToken cancellationToken)
    {
        var item = await GetOwnedItemAsync(context, itemId, cancellationToken);

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

    private static async Task<TodoItem> GetOwnedItemAsync(
        ApplicationDbContext context,
        Guid itemId,
        CancellationToken cancellationToken)
    {
        var item = await context.TodoItems
            .Include(item => item.List)
            .AsTracking()
            .FirstOrDefaultAsync(item => item.Id == itemId, cancellationToken);

        return item ?? throw new InvalidOperationException("Todo item not found.");
    }
}
