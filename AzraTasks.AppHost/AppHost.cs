using AzraTasks.AppHost;
using AzraTasks.Core;

var builder = DistributedApplication.CreateBuilder(args);

var db = builder.AddSqliteDatabase();

var backend = builder.AddProject<Projects.AzraTasks_Api>(Resources.Backend)
    .WithDependency(db, ConnectionStrings.DatabaseKey)
    .WithExternalHttpEndpoints()
    .WithGenApiClientCommand()
    //NB: We handle the migrations in the database resource.
    //Setting this to false to avoid wastefully applying migrations twice.
    .WithEnvironment("RunMigrationsOnStartup", bool.FalseString);

#pragma warning disable ASPIREBROWSERLOGS001 // WithBrowserLogs is still experimental
var frontendApp = builder.AddViteApp(Resources.Frontend, "../src/AzraTasks.Web")
    .WithExternalHttpEndpoints()
    .WithDependency(backend)
    .WithBrowserLogs()
    .WithEnvironment("VITE_BACKEND_HTTP", backend.GetEndpoint("http"))
    .WithEnvironment("VITE_BACKEND_HTTPS", backend.GetEndpoint("https"))
    ;
#pragma warning restore ASPIREBROWSERLOGS001 

builder.Build().Run();
