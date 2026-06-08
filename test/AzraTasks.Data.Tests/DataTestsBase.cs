namespace AzraTasks.Data.Tests;

public abstract class DataTestsBase
{
    protected AutoMocker Mocker { get; } = new();

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
}
