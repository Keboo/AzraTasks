using System.Diagnostics;

using Aspire.Hosting.Azure;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Logging;

using AzraTasks.AppHost;
using AzraTasks.Core;

namespace AzraTasks.AppHost;

public static class Resources
{
    public const string ContainerSuffixKey = "AzraTasks:ContainerSuffix";
    public const string Backend = "AzraTasks-api";
    public const string Frontend = "AzraTasks-frontend";
    public const string Database = "AzraTasks-database";

    extension(IDistributedApplicationBuilder builder)
    {
        public IResourceBuilder<SqliteResource> AddSqliteDatabase()
        {
            var database = builder.AddSqlite(Database, databaseFileName: $"{Database}.db");
            database.OnResourceReady(async (resource, e, cancellationToken) =>
            {
                if (!await ApplyDatabaseMigrationsAsync(resource, e.Services, cancellationToken))
                {
                    throw new Exception("Failed to apply database migrations to the database");
                }
            });
            database.WithDotnetToolRestoreCommand();

            database.WithCommand("CreateMigration", "Create Migration", async ctx =>
            {
                string? connectionString = await database.Resource.ConnectionStringExpression.GetValueAsync(ctx.CancellationToken);
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return CommandResults.Failure("No connection string to the database");
                }
#pragma warning disable ASPIREINTERACTION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                var interactionService = ctx.ServiceProvider.GetRequiredService<IInteractionService>();
                var migrationNameResult = await interactionService.PromptInputAsync("Migration Name", "Enter the name for the migration", "Name", "", cancellationToken: ctx.CancellationToken);
                if (migrationNameResult.Canceled || string.IsNullOrWhiteSpace(migrationNameResult.Data.Value))
                {
                    return CommandResults.Canceled();
                }
#pragma warning restore ASPIREINTERACTION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                ProcessStartInfo psi = new()
                {
                    FileName = "dotnet",
                    ArgumentList = {
                        "ef",
                        "migrations",
                        "--startup-project",
                        "./AzraTasks.AppHost",
                        "--project",
                        "./AzraTasks.Data",
                        "--no-build",
                        "add",
                        migrationNameResult.Data.Value
                    },
                    WorkingDirectory = GetSolutionDirectory()?.FullName,
                    EnvironmentVariables =
                    {
                        { $"ConnectionStrings__{ConnectionStrings.DatabaseKey}", connectionString }
                    }
                };
                bool processResult = await database.ExecuteProcessAsync(ctx, psi);
                return processResult ? CommandResults.Success() : CommandResults.Failure("Failed to create a migration");
            }, new CommandOptions()
            {
                IconName = "TableAdd"
            });

            database.WithCommand("RemoveMigration", "Remove Migration", async ctx =>
            {
#pragma warning disable ASPIREINTERACTION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                var interactionService = ctx.ServiceProvider.GetRequiredService<IInteractionService>();
                var confirmationResult = await interactionService.PromptConfirmationAsync("Remove Migration", "This will remove the most recent compiled migration. Continue?",
                    options: new()
                    {
                        PrimaryButtonText = "Yes",
                        SecondaryButtonText = "No",
                        Intent = MessageIntent.Warning
                    },
                    cancellationToken: ctx.CancellationToken);
                if (confirmationResult.Canceled || !confirmationResult.Data)
                {
                    return CommandResults.Canceled();
                }
#pragma warning restore ASPIREINTERACTION001 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
                string? connectionString = await database.Resource.ConnectionStringExpression.GetValueAsync(ctx.CancellationToken);
                if (string.IsNullOrWhiteSpace(connectionString))
                {
                    return CommandResults.Canceled();
                }
                ProcessStartInfo psi = new()
                {
                    FileName = "dotnet",
                    ArgumentList = {
                        "ef",
                        "migrations",
                        "--startup-project",
                        "./AzraTasks.AppHost",
                        "--project",
                        "./AzraTasks.Data",
                        "--no-build",
                        "remove"
                    },
                    WorkingDirectory = GetSolutionDirectory()?.FullName,
                    EnvironmentVariables =
                    {
                        { $"ConnectionStrings__{ConnectionStrings.DatabaseKey}", connectionString }
                    }
                };

                bool processResult = await database.ExecuteProcessAsync(ctx, psi);
                return processResult ? CommandResults.Success() : CommandResults.Failure("Failed to remove a migration");
            }, new CommandOptions()
            {
                IconName = "TableDismiss",
            });

            database.WithCommand("ApplyMigrations", "Apply Database Migrations",
                async ctx => await ApplyDatabaseMigrationsAsync(database.Resource, ctx.ServiceProvider, ctx.CancellationToken)
                    ? CommandResults.Success() : CommandResults.Failure("Failed to apply migrations"), new CommandOptions()
                    {
                        IconName = "DatabaseLightning"
                    });

            return database;
        }
    }

    extension<TResource>(IResourceBuilder<TResource> builder) where TResource : IResource
    {
        public IResourceBuilder<TResource> WithDotnetToolRestoreCommand()
        {
            builder.WithCommand("RestoreTools", "Restore Tools", async ctx =>
            {
                bool processResult = await RestoreDotnetToolsAsync(builder.Resource, ctx.ServiceProvider);
                return processResult ? CommandResults.Success() : CommandResults.Failure("Failed to restore tools");
            }, new CommandOptions()
            {
                IconName = "Toolbox"
            });
            return builder;
        }

        public IResourceBuilder<TResource> WithGenApiClientCommand()
        {
            builder.WithCommand("GenApiClient", "Generate API Client", async ctx =>
            {
                if (!builder.Resource.TryGetEndpoints(out var endpoints) || endpoints.FirstOrDefault() is not { } endpoint)
                {
                    return CommandResults.Failure("No external HTTP endpoint available to use to generate the API client from");
                }
                ProcessStartInfo psi = new()
                {
                    FileName = "pnpm",
                    ArgumentList = {
                        "run",
                        "openapi"
                    },
                    WorkingDirectory = Path.Combine(GetSolutionDirectory()!.FullName, "AzraTasks.Web"),
                };

                bool processResult = await builder.Resource.ExecuteProcessAsync(ctx.ServiceProvider, psi);
                return processResult ? CommandResults.Success() : CommandResults.Failure("Failed to generate API client");
            }, new CommandOptions()
            {
                IconName = "ChevronDoubleRight",
                UpdateState = ctx =>
                    ctx.ResourceSnapshot.HealthStatus == HealthStatus.Healthy
                        ? ResourceCommandState.Enabled : ResourceCommandState.Disabled
            });
            return builder;
        }
    }

    private static async Task<bool> RestoreDotnetToolsAsync(IResource resource, IServiceProvider services)
    {
        ProcessStartInfo psi = new()
        {
            FileName = "dotnet",
            ArgumentList = {
                    "tool",
                    "restore"
                },
            WorkingDirectory = GetSolutionDirectory()?.FullName,
        };

        return await resource.ExecuteProcessAsync(services, psi);
    }

    private static async Task<bool> ApplyDatabaseMigrationsAsync(IResourceWithConnectionString database, IServiceProvider services, CancellationToken cancellationToken)
    {
        string? connectionString = await database.ConnectionStringExpression.GetValueAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(connectionString)) throw new InvalidOperationException("Connection string for database not available");

        ILogger logger = services.GetResourceLogger(database);

        logger.LogInformation("Applying any pending migrations to the database");

        bool processResult = await ApplyMigrationsAsync();

        if (!processResult && await RestoreDotnetToolsAsync(database, services))
        {
            processResult = await ApplyMigrationsAsync();
        }

        if (processResult)
        {
            logger.LogInformation("Applied migrations to the database");
            return true;
        }
        else
        {
            logger.LogError("Failed to apply migrations to the database");
            return false;
        }

        Task<bool> ApplyMigrationsAsync()
        {
            // Determine the build configuration to use
            // Check common environment variable or default to Debug for local development
            string configuration = Environment.GetEnvironmentVariable("DOTNET_BUILD_CONFIGURATION") 
                ?? Environment.GetEnvironmentVariable("Configuration") 
                ?? "Debug";
            
            ProcessStartInfo psi = new()
            {
                FileName = "dotnet",
                ArgumentList = {
                    "ef",
                    "database",
                    "update",
                    "--no-build",
                    "--configuration",
                    configuration,
                    "--startup-project",
                    "./AzraTasks.AppHost",
                    "--project",
                    "./AzraTasks.Data",
                },
                WorkingDirectory = GetSolutionDirectory()?.FullName,
                EnvironmentVariables =
                {
                    { $"ConnectionStrings__{ConnectionStrings.DatabaseKey}", connectionString }
                }
            };
            return database.ExecuteProcessAsync(services, psi, cancellationToken);
        }
    }

    private static DirectoryInfo? GetSolutionDirectory()
    {
        for (DirectoryInfo dir = new(Directory.GetCurrentDirectory());
            dir.Parent is not null;
            dir = dir.Parent)
        {
            if (dir.EnumerateFiles("*.sln?").Any())
            { 
                return dir;
            }
        }
        return null;
    }
}