using System;
using System.Diagnostics;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Identity.Web;
using Microsoft.Graph;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using GraphApiSharepointIdentity.Models;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;

namespace GraphApiSharepointIdentity.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        readonly ITokenAcquisition tokenAcquisition;

        public HomeController(ITokenAcquisition tokenAcquisition, ILogger<HomeController> logger)
        {
            this.tokenAcquisition = tokenAcquisition;
            _logger = logger;
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Index()
        {
            var graphclient = await GetGraphClient();
            var user = await graphclient.Me.Request().GetAsync();
            ViewData["ApiResult"] = user.DisplayName;

            return View();
        }

        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> Profile()
        {
            var graphclient = await GetGraphClient();
            var me = await graphclient.Me.Request().GetAsync();
            ViewData["Me"] = me;

            try
            {
                // Get user photo
                using (var photoStream = await graphclient.Me.Photo.Content.Request().GetAsync())
                {
                    byte[] photoByte = ((MemoryStream)photoStream).ToArray();
                    ViewData["Photo"] = Convert.ToBase64String(photoByte);
                }
            }
            catch (System.Exception)
            {
                ViewData["Photo"] = null;
            }

            return View();
        }

        //[AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<IActionResult> SharepointFile()
        {
            var graphclient = await GetGraphClient();
            var me = await graphclient.Me.Request().GetAsync();
            ViewData["Me"] = me;

            try
            {
                // Get user photo
                //using (var photoStream = await _graphServiceClient.Me.Photo.Content.Request().GetAsync())
                //{
                //    byte[] photoByte = ((MemoryStream)photoStream).ToArray();
                //    ViewData["Photo"] = Convert.ToBase64String(photoByte);
                //}

                var url = "https://damienbodtestsharing.sharepoint.com/sites/TestDoc/Shared%20Documents/Forms/AllItems.aspx";

                var data = GetFile(url);
            }
            catch (System.Exception ex)
            {
                ViewData["Photo"] = null;
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

        private async Task<GraphServiceClient> GetGraphClient()
        {
            // Sharepoint "AllSites.FullControl" "AllSites.Read"
            //var token = await tokenAcquisition.GetAccessTokenForUserAsync(
            //    new string[] { "user.read", "AllSites.Read" });

            var token = await tokenAcquisition.GetAccessTokenForUserAsync(
                new string[] { "user.read" });

            GraphServiceClient graphClient = new GraphServiceClient("https://graph.microsoft.com/v1.0", 
                new DelegateAuthenticationProvider(async (requestMessage) =>
            {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
            }));

            return graphClient;
        }

        private async Task<string> GetFile(string sharepointUrl)
        {
            try
            {
                var graphclient = await GetGraphClient();
                var user = await graphclient.Me.Request().GetAsync();

                
                if (user == null)
                    throw new NotFoundException($"User not found in AD.");

                var sharepointDomain = "damienbodtestsharing.sharepoint.com";
                var relativePath = "/sites/TestDoc";
                // var folderToUse = "";
                var fileName = "aad_ms_login_02.png";

                var site = await graphclient
                    .Sites[sharepointDomain]
                    .SiteWithPath(relativePath)
                    .Request()
                    .GetAsync();

                var drive = await graphclient
                    .Sites[site.Id]
                    .Drive
                    .Request()
                    .GetAsync();

                var items = await graphclient
                    .Sites[site.Id]
                    .Drives[drive.Id]
                    .Root
                    .Children
                    .Request().GetAsync();

                var file = items
                    .FirstOrDefault(f => f.File != null && f.WebUrl.Contains(fileName));

                var stream = await graphclient
                    .Sites[site.Id]
                    .Drives[drive.Id]
                    .Items[file.Id].Content
                    .Request()
                    .GetAsync();

                var fileAsString = StreamToString(stream);
                return fileAsString;
                // folder to upload to
                //var folder = items
                //    .FirstOrDefault(f => f.Folder != null && f.WebUrl.Contains(folderToUse));

                //string fileNames = string.Empty;
                //var files = await _graphServiceClient
                //    .Sites[site.Id]
                //    .Drives[drive.Id]
                //    .Items[folder.Id]
                //    .Children
                //    .Request().GetAsync();

                //foreach (var file in files)
                //{
                //    fileNames = $"{fileNames} {file.Name}";

                //    var stream = await graphClient
                //        .Sites[site.Id]
                //        .Drives[drive.Id]
                //        .Items[file.Id].Content
                //        .Request()
                //        .GetAsync();

          
                //}
            }
            catch (Exception ex)
            {
                string dd = ex.Message;
            }

            return "TODO";
        }

        private static string StreamToString(Stream stream)
        {
            stream.Position = 0;
            using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
            {
                return reader.ReadToEnd();
            }
        }
    }
}
