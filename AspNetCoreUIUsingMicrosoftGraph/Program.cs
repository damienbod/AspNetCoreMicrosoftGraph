using GraphApiSharepointIdentity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Identity.Web.UI;
using Microsoft.IdentityModel.Logging;
using System.IdentityModel.Tokens.Jwt;

var builder = WebApplication.CreateBuilder(args);

string[]? initialScopes = builder.Configuration.GetValue<string>("CallApi:Scopes")?.Split(' ');

builder.Services.AddHttpClient();
builder.Services.AddScoped<GraphApiClientUI>();
builder.Services.AddScoped<ApiService>();

var baseAddress = builder.Configuration["GraphApi:BaseUrl"];
baseAddress ??= "https://graph.microsoft.com/beta";

builder.Services.AddMicrosoftIdentityWebAppAuthentication(builder.Configuration)
    .EnableTokenAcquisitionToCallDownstreamApi(initialScopes)
    .AddMicrosoftGraph(baseAddress, "https://graph.microsoft.com/.default")
    .AddInMemoryTokenCaches();

builder.Services.AddControllersWithViews(options =>
{
    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.Filters.Add(new AuthorizeFilter(policy));
}).AddMicrosoftIdentityUI();

builder.Services.AddRazorPages();

var app = builder.Build();

JwtSecurityTokenHandler.DefaultInboundClaimTypeMap.Clear();
IdentityModelEventSource.ShowPII = true;

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllerRoute(name: "default", pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();