[![.NET](https://github.com/damienbod/AspNetCoreMicrosoftGraph/actions/workflows/dotnet.yml/badge.svg)](https://github.com/damienbod/AspNetCoreMicrosoftGraph/actions/workflows/dotnet.yml)

# Using Microsoft Graph API delegated clients in ASP.NET Core

https://damienbod.com/2020/11/20/using-microsoft-graph-api-in-asp-net-core/

## API permissions

Sites.Read.All delegated

## Graph API

```csharp
builder.Services.AddMicrosoftIdentityWebApiAuthentication(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi()
    .AddMicrosoftGraph(builder.Configuration.GetSection("GraphApi"))
    .AddInMemoryTokenCaches();
```

appsettings.json

```json
"GraphApi": {
    "ClientId": "89cbcff9-7c4e-4659-9948-d6f7fda186e1",
    "Scopes": "user.read Sites.Read.All"
},
 ```

## History

2023-08-22 Updated to Graph 5 

2023-08-22 Updated packages and startup

2023-03-02 Updated packages

2023-01-15 Updated packages to .NET 7

2022-06-10 Updated packages

2022-01-28 Updated packages

2021-12-15 Updated packages

2021-10-29 Updated to .NET 6, Microsoft.Identity.Web 1.18.0

2021-02-28 Updated to Microsoft.Identity.Web 1.7.0

2021-02-13 Updated to Microsoft.Identity.Web 1.6.0

## Links

https://developer.microsoft.com/en-us/graph/

https://docs.microsoft.com/en-us/aspnet/core/fundamentals/http-requests

https://docs.microsoft.com/en-us/dotnet/api/system.net.http.httpclient
