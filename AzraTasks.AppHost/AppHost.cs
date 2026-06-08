using AzraTasks.AppHost;
using AzraTasks.Core;

using Microsoft.Extensions.DependencyInjection;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddSqliteDatabase();

var backend = builder.AddProject<Projects.AzraTasks_Api>(Resources.Backend)
    .WithDependency(db, ConnectionStrings.DatabaseKey)
    .WithExternalHttpEndpoints()
    .WithGenApiClientCommand();

#pragma warning disable ASPIREBROWSERLOGS001 
var frontendApp = builder.AddViteApp(Resources.Frontend, "../src/AzraTasks.Web")
    .WithExternalHttpEndpoints()
    .WithDependency(backend)
    .WithBrowserLogs()
    .WithEnvironment("VITE_BACKEND_HTTP", backend.GetEndpoint("http"))
    .WithEnvironment("VITE_BACKEND_HTTPS", backend.GetEndpoint("https"))
    ;
#pragma warning restore ASPIREBROWSERLOGS001 

if (builder.ExecutionContext.IsPublishMode)
{
    // Enable migrations on startup for Azure deployments
    // Applying migrations on startup is not recommended for production scenarios.
    // See: https://learn.microsoft.com/ef/core/managing-schemas/migrations/applying?tabs=dotnet-core-cli&WT.mc_id=DT-MVP-5003472
    backend.WithEnvironment("RunMigrationsOnStartup", "true");
}

builder.Build().Run();
