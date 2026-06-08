using AzraTasks.Data;

namespace AzraTasks.Testing;

public abstract class UnitTestBase
{
    protected AutoMocker Mocker { get; } = new();

    protected static CancellationToken CT => TestContext.Current!.Execution.CancellationToken;

    [Before(Test)]
    public async Task Setup()
    {
        Mocker.WithDbContext<ApplicationDbContext>(
            new TrackingBaseInterceptor(),
            new CreatedByUserInterceptor()
        );
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

        Mocker.WithUser(user);
        return user;
    }

    protected async Task<TodoList> CreateTodoList(string userId, string name = "Test List")
    {
        var list = new TodoList
        {
            Id = Guid.NewGuid(),
            Name = name,
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
