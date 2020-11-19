using GraphApiSharepointIdentity.Controllers;
using Microsoft.Extensions.Logging;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace GraphApiSharepointIdentity
{
    public class GraphApiClient
    {
        private readonly ILogger<GraphApiClient> _logger;

        readonly ITokenAcquisition tokenAcquisition;

        public GraphApiClient(ITokenAcquisition tokenAcquisition, 
            ILogger<GraphApiClient> logger)
        {
            this.tokenAcquisition = tokenAcquisition;
            _logger = logger;
        }

        public async Task<User> GetGraphApiUser()
        {
            var graphclient = await GetGraphClient(new string[] { "user.read" })
               .ConfigureAwait(false);

            return await graphclient.Me.Request().GetAsync().ConfigureAwait(false);
        }

        public async Task<string> GetGraphApiProfilePhoto()
        {
            var graphclient = await GetGraphClient(new string[] { "user.read" })
               .ConfigureAwait(false);

            var photo = string.Empty;
            // Get user photo
            using (var photoStream = await graphclient.Me.Photo
                .Content.Request().GetAsync().ConfigureAwait(false))
            {
                byte[] photoByte = ((MemoryStream)photoStream).ToArray();
                photo = Convert.ToBase64String(photoByte);
            }

            return photo;
        }

        public async Task<string> GetSharepointFile()
        {
            var graphclient = await GetGraphClient(
                new string[] { "user.read", "AllSites.Read" }
            ).ConfigureAwait(false);

            var user = await graphclient.Me.Request().GetAsync().ConfigureAwait(false);

            if (user == null)
                throw new NotFoundException($"User not found in AD.");

            var sharepointDomain = "damienbodtestsharing.sharepoint.com";
            var relativePath = "/sites/TestDoc";
            var fileName = "aad_ms_login_02.png";

            var site = await graphclient
                .Sites[sharepointDomain]
                .SiteWithPath(relativePath)
                .Request()
                .GetAsync().ConfigureAwait(false);

            var drive = await graphclient
                .Sites[site.Id]
                .Drive
                .Request()
                .GetAsync().ConfigureAwait(false);

            var items = await graphclient
                .Sites[site.Id]
                .Drives[drive.Id]
                .Root
                .Children
                .Request().GetAsync().ConfigureAwait(false);

            var file = items
                .FirstOrDefault(f => f.File != null && f.WebUrl.Contains(fileName));

            var stream = await graphclient
                .Sites[site.Id]
                .Drives[drive.Id]
                .Items[file.Id].Content
                .Request()
                .GetAsync().ConfigureAwait(false);

            var fileAsString = StreamToString(stream);
            return fileAsString;
        }

        private async Task<GraphServiceClient> GetGraphClient(string[] scopes)
        {
            var token = await tokenAcquisition.GetAccessTokenForUserAsync(
               scopes).ConfigureAwait(false);

            GraphServiceClient graphClient = new GraphServiceClient("https://graph.microsoft.com/v1.0",
                new DelegateAuthenticationProvider(async (requestMessage) =>
                {
                    requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                }));

            return graphClient;
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
