using System.Diagnostics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Mvc;
using GraphApiSharepointIdentity.Models;

namespace GraphApiSharepointIdentity.Controllers;

[Authorize]
public class HomeController : Controller
{
    private readonly GraphApiClientUI _graphApiClientUI;

    public HomeController(GraphApiClientUI graphApiClientUI)
    {
        _graphApiClientUI = graphApiClientUI;
    }

    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public async Task<IActionResult> Index()
    {
        var user = await _graphApiClientUI.GetGraphApiUser();

        ViewData["ApiResult"] = user.DisplayName;

        return View();
    }

    [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
    public async Task<IActionResult> Profile()
    {
        var user = await _graphApiClientUI.GetGraphApiUser();

        ViewData["Me"] = user;

        try
        {
            ViewData["Photo"] = await _graphApiClientUI.GetGraphApiProfilePhoto(user!.Id!);
        }
        catch
        {
            ViewData["Photo"] = null;
        }

        return View();
    }

    public async Task<IActionResult> SharepointFile()
    {
        try
        {
            var data = await _graphApiClientUI.GetSharepointFile();
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
        }

        return View();
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [AllowAnonymous]
    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
