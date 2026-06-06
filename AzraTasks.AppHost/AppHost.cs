using AzraTasks.AppHost;
using AzraTasks.Core;

using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddSqliteDatabase();

var backend = builder.AddProject<Projects.AzraTasks>("AzraTasks-backend")
    .WithDependency(db, ConnectionStrings.DatabaseKey)
    .WithUITests()
    .WithExternalHttpEndpoints()
    .PublishAsAzureContainerApp((infra, app) => app.Template.Scale.MaxReplicas = 1);

var frontendApp = builder.AddJavaScriptApp(Resources.Frontend, "../AzraTasks.Web", "dev")
    .WithNpm(install: true)
    .WithHttpEndpoint(env: "PORT")
    .WithExternalHttpEndpoints()
    .WithDependency(backend)
    .WithEnvironment("REACTAPP_BACKEND_HTTP", backend.GetEndpoint("http"))
    .WithEnvironment("REACTAPP_BACKEND_HTTPS", backend.GetEndpoint("https"));

if (builder.ExecutionContext.IsPublishMode)
{
    // Enable migrations on startup for Azure deployments
    // Applying migrations on startup is not recommended for production scenarios.
    // See: https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli&WT.mc_id=DT-MVP-5003472
    backend.WithEnvironment("RunMigrationsOnStartup", "true");
}

builder.Build().Run();
