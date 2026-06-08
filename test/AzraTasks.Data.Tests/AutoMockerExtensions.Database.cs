using AzraTasks.Data.Auth;

using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;

using Moq.AutoMock.Resolvers;

namespace AzraTasks.Data.Tests;

public static partial class AutoMockerExtensions
{
    extension(AutoMocker mocker)
    {
        public void WithUser(ApplicationUser user) => mocker.WithUser(user.Id);

        public void WithUser(string userId)
        {
            var resolver = mocker.Resolvers.OfType<SqliteConnectionResolver<ApplicationDbContext>>().Single();
            resolver.UserId = userId;
        }

        public void WithDbContext<TContext>(params IInterceptor[] interceptors)
            where TContext : DbContext
        {
            SqliteConnectionResolver<TContext> sqliteResolver = new(interceptors);
            // NB: Injecting the resolver so it can be cleaned up with Mocker.AsDisposable()
            mocker.Use(sqliteResolver);
            mocker.Resolvers.Insert(0, sqliteResolver);
        }

        public async Task InDbScopeAsync(Func<ApplicationDbContext, Task> action)
        {
            using var context = mocker.Get<ApplicationDbContext>();
            await action(context);
        }

        public async Task<T> InDbScopeAsync<T>(Func<ApplicationDbContext, Task<T>> action)
        {
            using var context = mocker.Get<ApplicationDbContext>();
            return await action(context);
        }
    }
}

file sealed class SqliteConnectionResolver<TContext> : IUserIdProvider, IMockResolver, IDisposable
    where TContext : DbContext
{
    public const string CurrentDateTimeOffset = "SYSDATETIMEOFFSET";

    private readonly Lazy<SqliteConnection> _sqliteConnection = new(() =>
    {
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.CreateFunction(CurrentDateTimeOffset, () => DateTimeOffset.UtcNow);

        connection.Open();
        return connection;
    });
    private bool _disposedValue;
    private readonly IInterceptor[] _interceptors;

    public string UserId { get; set; } = "";

    public SqliteConnectionResolver(IInterceptor[] interceptors)
    {
        _interceptors = interceptors;
    }

    public void Resolve(MockResolutionContext context)
    {
        if (context.RequestType == typeof(DbContextOptions<TContext>))
        {
            var options = GetOptions();
            context.Value = options;
        }
        else if (context.RequestType == typeof(TContext))
        {
            context.Value = CreateDbContext(context.AutoMocker);
        }
        else if (context.RequestType == typeof(IDbContextFactory<TContext>))
        {
            Mock<IDbContextFactory<TContext>> factory = new();
            factory.Setup(x => x.CreateDbContext())
                   .Returns(() => CreateDbContext(context.AutoMocker));
            factory.Setup(x => x.CreateDbContextAsync(It.IsAny<CancellationToken>()))
                   .ReturnsAsync(() => CreateDbContext(context.AutoMocker));
            context.Value = factory.Object;
        }

        TContext CreateDbContext(AutoMocker autoMocker)
        {
            var options = autoMocker.Get<DbContextOptions<TContext>>();
            IUserIdProvider? userIdProvider = this;
            var dbContext = (TContext)Activator.CreateInstance(typeof(TContext), options, userIdProvider)!;
            dbContext.Database.EnsureCreated();
            return dbContext;
        }

        DbContextOptions<TContext> GetOptions()
        {
            var builder = new DbContextOptionsBuilder<TContext>()
                    .EnableDetailedErrors()
                    .EnableSensitiveDataLogging()
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                    .AddInterceptors(_interceptors)
                    .UseSqlite(_sqliteConnection.Value);

            // Bit of a hack to see if a logging factory mock resolver was registered.
            if (context.AutoMocker.Resolvers.OfType<ILoggerFactory>().Any())
            {
                builder.UseLoggerFactory(context.AutoMocker.Get<ILoggerFactory>());
            }

            return builder.Options;
        }
    }

    private void Dispose(bool disposing)
    {
        if (!_disposedValue)
        {
            if (disposing)
            {
                if (_sqliteConnection.IsValueCreated)
                {
                    _sqliteConnection.Value.Dispose();
                }
            }

            _disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
