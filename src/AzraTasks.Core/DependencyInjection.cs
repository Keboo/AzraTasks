using AzraTasks.Core.Auth;
using AzraTasks.Core.Todos;
using AzraTasks.Data;
using AzraTasks.Data.Auth;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;

namespace AzraTasks.Core;

public static class DependencyInjection
{
    public static TBuilder AddDatabase<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.AddSqliteDbContext<ApplicationDbContext>(ConnectionStrings.DatabaseKey, 
            configureDbContextOptions: options =>
            {
                options.AddInterceptors(
                    new TrackingBaseInterceptor(),
                    new CreatedByUserInterceptor()
                );
            });

        if (builder.Environment.IsDevelopment())
        {
            builder.Services.AddDatabaseDeveloperPageExceptionFilter();
        }

        builder.Services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.SignIn.RequireConfirmedAccount = true;
            options.Stores.SchemaVersion = IdentitySchemaVersions.Version3;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddSignInManager()
            .AddDefaultTokenProviders();

        // Support both migration flags while preferring the explicit positive form.
        // RunMigrationsOnStartup=true enables startup migrations.
        // SkipMigrationsOnStartup=true disables startup migrations (legacy inverse flag).
        // Default is disabled when neither setting is provided.
        bool? runMigrationsOnStartup = builder.Configuration.GetValue<bool?>("RunMigrationsOnStartup");

        // Applying migrations on startup is not recommended for production scenarios.
        // See: https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli&WT.mc_id=DT-MVP-5003472
        if (runMigrationsOnStartup != false && 
            builder.Configuration.GetConnectionString(ConnectionStrings.DatabaseKey) is { Length: > 0 })
        {
            builder.Services.AddHostedService<DatabaseMigrationService>();
        }

        builder.Services.TryAddSingleton<IUserIdProvider, NullUserIdProvider>();

        return builder;
    }

    public static TBuilder AddTodo<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddScoped<ITodoListService, TodoListService>();

        return builder;
    }

}