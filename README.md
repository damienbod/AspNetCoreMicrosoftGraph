[![.NET](https://github.com/damienbod/AspNetCoreMicrosoftGraph/actions/workflows/dotnet.yml/badge.svg)](https://github.com/damienbod/AspNetCoreMicrosoftGraph/actions/workflows/dotnet.yml)

# Using Microsoft Graph API delegated clients in ASP.NET Core

https://damienbod.com/2020/11/20/using-microsoft-graph-api-in-asp-net-core/

## Delegated API permissions

Sites.Read.All 

## Graph API setup in Web API project

```csharp
builder.Services.AddScoped<GraphApiClientDirect>();

builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph()
    .AddInMemoryTokenCaches();
```

appsettings.json

```json
  "AzureAd": {
    "Instance": "https://login.microsoftonline.com/",
    "Domain": "damienbodsharepoint.onmicrosoft.com",
    "CallbackPath": "/signin-oidc",
    "ClientId": "89cbcff9-7c4e-4659-9948-d6f7fda186e1", // sharepoint graph api
    "TenantId": "5698af84-5720-4ff0-bdc3-9d9195314244"
    // Secret is required for the downstream Graph API call
    // secret is not required for the API itself
    //"ClientSecret": "--secret-in-user-secrets--"
  },
 ```

Graph service from API for OBO flow, downstream API

 ```csharp
private readonly GraphServiceClient _graphServiceClient;

// "user.read Sites.Read.All" consented in the App registration
// The default scope is used because this is a downstream API OBO delegated user flow
private const string SCOPES = "https://graph.microsoft.com/.default";

public GraphApiClientDirect(GraphServiceClient graphServiceClient)
{
    // https://graph.microsoft.com/.default
    // "user.read Sites.Read.All" consented in the App registration
    _graphServiceClient = graphServiceClient;
}
 ```

## History

- 2023-12-02 Fix photo streaming
- 2023-11-22 Updated .NET 8
- 2023-08-26 Improved code, added comments
- 2023-08-22 Updated to Graph 5 
- 2023-08-22 Updated packages and startup
- 2023-03-02 Updated packages
- 2023-01-15 Updated packages to .NET 7
- 2022-06-10 Updated packages
- 2022-01-28 Updated packages
- 2021-12-15 Updated packages
- 2021-10-29 Updated to .NET 6, Microsoft.Identity.Web 1.18.0
- 2021-02-28 Updated to Microsoft.Identity.Web 1.7.0
- 2021-02-13 Updated to Microsoft.Identity.Web 1.6.0

## Links

https://developer.microsoft.com/en-us/graph/

https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests

https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient
