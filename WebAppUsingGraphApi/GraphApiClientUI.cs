using GraphApiSharepointIdentity.Controllers;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GraphApiSharepointIdentity
{
    public class GraphApiClientUI
    {
        private readonly GraphServiceClient _graphServiceClient;

        public GraphApiClientUI(ITokenAcquisition tokenAcquisition,
            GraphServiceClient graphServiceClient)
        {
            _graphServiceClient = graphServiceClient;
        }

        public async Task<User> GetGraphApiUser()
        {
            return await _graphServiceClient.Me.Request().GetAsync().ConfigureAwait(false);
        }

        public async Task<string> GetGraphApiProfilePhoto()
        {
            var photo = string.Empty;
            // Get user photo
            using (var photoStream = await _graphServiceClient.Me.Photo
                .Content.Request().GetAsync().ConfigureAwait(false))
            {
                byte[] photoByte = ((MemoryStream)photoStream).ToArray();
                photo = Convert.ToBase64String(photoByte);
            }

            return photo;
        }

        public async Task<string> GetSharepointFile()
        {
            var user = await _graphServiceClient.Me.Request().GetAsync().ConfigureAwait(false);

            if (user == null)
                throw new NotFoundException($"User not found in AD.");

            var sharepointDomain = "damienbodtestsharing.sharepoint.com";
            var relativePath = "/sites/TestDoc";
            var fileName = "aad_ms_login_02.png";

            var site = await _graphServiceClient
                .Sites[sharepointDomain]
                .SiteWithPath(relativePath)
                .Request()
                .GetAsync().ConfigureAwait(false);

            var drive = await _graphServiceClient
                .Sites[site.Id]
                .Drive
                .Request()
                .GetAsync().ConfigureAwait(false);

            var items = await _graphServiceClient
                .Sites[site.Id]
                .Drives[drive.Id]
                .Root
                .Children
                .Request().GetAsync().ConfigureAwait(false);

            var file = items
                .FirstOrDefault(f => f.File != null && f.WebUrl.Contains(fileName));

            var stream = await _graphServiceClient
                .Sites[site.Id]
                .Drives[drive.Id]
                .Items[file.Id].Content
                .Request()
                .GetAsync().ConfigureAwait(false);

            var fileAsString = StreamToString(stream);
            return fileAsString;
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
