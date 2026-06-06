using AzraTasks.Core.Todos;
using AzraTasks.Data;

using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace AzraTasks.Core;

public static class DependencyInjection
{
    public static TBuilder AddDatabase<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.AddSqliteDbContext<ApplicationDbContext>(ConnectionStrings.DatabaseKey);

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

        // Only run migrations on startup when explicitly enabled (e.g., during: azd up)
        // Applying migrations on startup is not recommended for production scenarios.
        // See: https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli&WT.mc_id=DT-MVP-5003472
        if (builder.Configuration.GetValue<bool>("RunMigrationsOnStartup"))
        {
            builder.Services.AddHostedService<DatabaseMigrationService>();
        }

        return builder;
    }

    public static TBuilder AddTodo<TBuilder>(this TBuilder builder) where TBuilder : IHostApplicationBuilder
    {
        builder.Services.AddScoped<ITodoListService, TodoListService>();

        return builder;
    }

}