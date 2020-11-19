using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.AspNetCore.Mvc;
using GraphApiSharepointIdentity.Models;

namespace GraphApiSharepointIdentity.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly GraphApiClient _graphApiClient;

        public HomeController(GraphApiClient graphApiClient)
        {
            _graphApiClient = graphApiClient;
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Index()
        {
            var user = await _graphApiClient.GetGraphApiUser()
                .ConfigureAwait(false);

            ViewData["ApiResult"] = user.DisplayName;

            return View();
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Profile()
        {
            var user = await _graphApiClient.GetGraphApiUser()
                .ConfigureAwait(false);

            ViewData["Me"] = user;

            try
            {
                ViewData["Photo"] = _graphApiClient.GetGraphApiProfilePhoto();
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
                var data = await _graphApiClient.GetSharepointFile().ConfigureAwait(false);
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
}
