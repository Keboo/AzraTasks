using AzraTasks.Data;

using Microsoft.EntityFrameworkCore;

namespace AzraTasks.Core.Todos;

public class TodoListService(IDbContextFactory<ApplicationDbContext> contextFactory) : ITodoListService
{
    public async Task<IEnumerable<Room>> GetListsAsync(string userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Rooms
            .Where(room => room.CreatedByUserId == userId)
            .OrderByDescending(room => room.CreatedDate)
            .ToListAsync();
    }

    public async Task<Room?> GetListByIdAsync(Guid listId, string userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();
        return await context.Rooms
            .FirstOrDefaultAsync(room => room.Id == listId && room.CreatedByUserId == userId);
    }

    public async Task<Room> CreateListAsync(string name, string userId, CancellationToken cancellationToken)
    {
        var normalizedName = NormalizeListName(name);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var exists = await context.Rooms
            .AnyAsync(
                room => room.CreatedByUserId == userId && EF.Functions.Like(room.FriendlyName, normalizedName),
                cancellationToken);

        if (exists)
        {
            throw new InvalidOperationException($"You already have a list named '{normalizedName}'.");
        }

        var room = new Room
        {
            Id = Guid.NewGuid(),
            FriendlyName = normalizedName,
            CreatedByUserId = userId,
            CreatedDate = DateTimeOffset.UtcNow
        };

        context.Rooms.Add(room);
        await context.SaveChangesAsync(cancellationToken);

        return room;
    }

    public async Task DeleteListAsync(Guid listId, string userId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);

        var room = await context.Rooms
            .Include(existingRoom => existingRoom.Questions)
            .FirstOrDefaultAsync(
                existingRoom => existingRoom.Id == listId && existingRoom.CreatedByUserId == userId,
                cancellationToken)
            ?? throw new InvalidOperationException("Todo list not found.");

        context.Questions.RemoveRange(room.Questions);
        context.Rooms.Remove(room);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<IEnumerable<Question>> GetItemsAsync(Guid listId, string userId)
    {
        await using var context = await contextFactory.CreateDbContextAsync();

        var listExists = await context.Rooms
            .AnyAsync(room => room.Id == listId && room.CreatedByUserId == userId);

        if (!listExists)
        {
            throw new InvalidOperationException("Todo list not found.");
        }

        return await context.Questions
            .Where(item => item.RoomId == listId)
            .OrderBy(item => item.IsAnswered)
            .ThenByDescending(item => item.CreatedDate)
            .ToListAsync();
    }

    public async Task<Question> CreateItemAsync(Guid listId, string title, string userId, CancellationToken cancellationToken)
    {
        var normalizedTitle = NormalizeItemTitle(title);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        await EnsureOwnedListExistsAsync(context, listId, userId, cancellationToken);

        var question = new Question
        {
            Id = Guid.NewGuid(),
            RoomId = listId,
            QuestionText = normalizedTitle,
            CreatedDate = DateTimeOffset.UtcNow
        };

        context.Questions.Add(question);
        await context.SaveChangesAsync(cancellationToken);

        return question;
    }

    public async Task<Question> UpdateItemAsync(Guid listId, Guid itemId, string title, string userId, CancellationToken cancellationToken)
    {
        var normalizedTitle = NormalizeItemTitle(title);

        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var item = await GetOwnedItemAsync(context, listId, itemId, userId, cancellationToken);

        item.QuestionText = normalizedTitle;
        item.LastModifiedDate = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<Question> SetItemCompletedAsync(Guid listId, Guid itemId, bool isCompleted, string userId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var item = await GetOwnedItemAsync(context, listId, itemId, userId, cancellationToken);

        item.IsAnswered = isCompleted;
        item.LastModifiedDate = DateTimeOffset.UtcNow;

        await context.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task DeleteItemAsync(Guid listId, Guid itemId, string userId, CancellationToken cancellationToken)
    {
        await using var context = await contextFactory.CreateDbContextAsync(cancellationToken);
        var item = await GetOwnedItemAsync(context, listId, itemId, userId, cancellationToken);

        context.Questions.Remove(item);
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
        var listExists = await context.Rooms
            .AnyAsync(room => room.Id == listId && room.CreatedByUserId == userId, cancellationToken);

        if (!listExists)
        {
            throw new InvalidOperationException("Todo list not found.");
        }
    }

    private static async Task<Question> GetOwnedItemAsync(
        ApplicationDbContext context,
        Guid listId,
        Guid itemId,
        string userId,
        CancellationToken cancellationToken)
    {
        var item = await context.Questions
            .Include(question => question.Room)
            .AsTracking()
            .FirstOrDefaultAsync(
                question => question.Id == itemId
                    && question.RoomId == listId
                    && question.Room!.CreatedByUserId == userId,
                cancellationToken);

        return item ?? throw new InvalidOperationException("Todo item not found.");
    }
}
