# AzraTaks
A small to-do task management app, built with ASP.NET + Vue.
The structure was heavily influences by my [Aspire React dotnet template](https://github.com/Keboo/DotnetTemplates).

The project is deployed to Azure and the running instance can be found at: https://black-moss-077b5271e.7.azurestaticapps.net/

# Running the project locally
Though the project leverage [Aspire](https://aspire.dev). By launching the app with the Aspire host, it will handle restoring all packages, starting the frontend vite server, starting the backend ASP.NET Core server, and linking everything together.

## Prerequisites

- [.NET SDK v10.0.x](https://get.dot.net)
- [Node v26.x](https://nodejs.org/)

## Launch with the Aspire CLI
If the [Aspire CLI](https://aspire.dev/reference/cli/overview/) is installed, you can simply run `aspire run` from the root of the repository.

## Launch with Visual Studio 2026
- Open the AzraTasks.slnx solution file
- Ensure the AzraTasks.AppHost is set as the startup project
- Press F5 to start the application

## Launch with Visual Studio Code
- 

## Launch from the terminal
From the root of the repository run `dotnet build` then `dotnet run --project AzraTasks.AppHost/AzraTasks.AppHost.csproj`


# A high-level tour

## Infrastructure
This app is being built and deployed onto Azure infrastructure.

```mermaid
flowchart LR
    U[Users] --> SWA Azure Static Web App\n(Frontend)
    SWA -->|HTTPS API calls| Azure Container App\n(Backend API)

    subgraph CAE[Azure Container Apps Environment (CAE)]
      CA
    end

    ACR[Azure Container Registry (ACR)] -->|Serves backend image| CA

    MI[User-Assigned Managed Identity\n(azratasks-*-mi)] -. attached to .-> CA
    MI -. used for registry auth .-> ACR
    MI -. AcrPull role assignment .-> ACR

    classDef highlight fill:#fff3cd,stroke:#ff9800,stroke-width:2px,color:#333;
    class MI,ACR,CA highlight;
```

## Backend and Database
The backend components are separated into the Api, Core, Data, and ServiceDefaults projects. Though all of these could be collapsed into a single project, it would mitigate many of the benefits of being able to easily share code with any additional services.
- The Api project is an ASP.NET Core project using [minimal APIs](https://learn.microsoft.com/aspnet/core/fundamentals/minimal-apis). It focuses on the API interactions and auth.
- The Core project contains the business logic of the application. 
- The Data project contains all of the EF Models, DbContext, and related code (such as interceptors) for interacting with the database. 
- The Service Defaults project is the [Aspire generated Service Defaults project](https://aspire.dev/get-started/csharp-service-defaults/).

## Frontend
The frontend application is inside of the AzraTasks.Web project. It is leveraging [Hep API](https://heyapi.dev/) to generate a TypeScript client for the backend using the published OpenAPI spec. 

The authentication is just simple web auth

## Testing

# Assumptions + Potential Future Improvements
