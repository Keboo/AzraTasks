using AzraTasks.Data;

namespace AzraTasks.Core.Tests;

public abstract class ServiceTestsBase
{
    protected AutoMocker Mocker { get; } = new();

    [Before(Test)]
    public async Task Setup()
    {
        Mocker.WithDbContext<ApplicationDbContext>();
    }

    [After(Test)]
    public async Task TearDown()
    {
        if (Mocker.AsDisposable() is { } disposable)
        {
            disposable.Dispose();
        }
    }

    protected async Task<ApplicationUser> CreateUserAsync(string userId = "test-user")
    {
        var user = new ApplicationUser
        {
            Id = $"{userId}-{Guid.NewGuid()}",
            UserName = $"{userId}@example.com",
            Email = $"{userId}@example.com"
        };
        
        await Mocker.InDbScopeAsync(async context =>
        {
            context.Users.Add(user);
            await context.SaveChangesAsync();
        });
        return user;
    }

    protected async Task<TodoList> CreateTodoList(string userId, string friendlyName = "Test Room")
    {
        var list = new TodoList
        {
            Id = Guid.NewGuid(),
            Name = friendlyName,
            CreatedById = userId,
            CreatedDate = DateTimeOffset.UtcNow
        };

        await Mocker.InDbScopeAsync(async context =>
        {
            context.TodoLists.Add(list);
            await context.SaveChangesAsync();
        });

        return list;
    }

    protected async Task<TodoItem> CreateTodoItemAsync(
        Guid listId,
        string text = "Test Item",
        bool isComplete = false)
    {
        var todoItem = new TodoItem
        {
            Id = Guid.NewGuid(),
            ListId = listId,
            Text = text,
            IsComplete = isComplete,
            CreatedDate = DateTimeOffset.UtcNow
        };

        await Mocker.InDbScopeAsync(async context =>
        {
            context.TodoItems.Add(todoItem);
            await context.SaveChangesAsync();
        });

        return todoItem;
    }
}
