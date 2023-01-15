using Microsoft.Graph;
using Microsoft.Identity.Web;
using System.Net.Http.Headers;
using System.Text;

namespace WebApiUsingGraphApi;


public class GraphApiClientDirect
{
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IHttpClientFactory _clientFactory;

    public GraphApiClientDirect(ITokenAcquisition tokenAcquisition,
        IHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
        _tokenAcquisition = tokenAcquisition;
    }

    public async Task<User> GetGraphApiUser()
    {
        var graphclient = await GetGraphClient(new string[] { "User.ReadBasic.All", "user.read" });

        return await graphclient.Me.Request().GetAsync();
    }

    public async Task<string> GetGraphApiProfilePhoto()
    {
        try
        {
            var graphclient = await GetGraphClient(new string[] { "User.ReadBasic.All", "user.read" });

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
        catch
        {
            return string.Empty;
        }   
    }

    public async Task<string> GetSharepointFile()
    {
        var graphclient = await GetGraphClient(
            new string[] { "user.read", "AllSites.Read" }
        );

        var user = await graphclient.Me.Request().GetAsync();

        if (user == null)
            throw new ArgumentException($"User not found in AD.");

        var sharepointDomain = "damienbodtestsharing.sharepoint.com";
        var relativePath = "/sites/TestDoc";
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
            .Request()
            .GetAsync();

        var file = items.FirstOrDefault(f => f.File != null && f.WebUrl.Contains(fileName));

        var stream = await graphclient
            .Sites[site.Id]
            .Drives[drive.Id]
            .Items[file!.Id].Content
            .Request()
            .GetAsync();

        var fileAsString = StreamToString(stream);
        return fileAsString;
    }

    private async Task<GraphServiceClient> GetGraphClient(string[] scopes)
    {
        var token = await _tokenAcquisition.GetAccessTokenForUserAsync(scopes);

        var client = _clientFactory.CreateClient();
        client.BaseAddress = new Uri("https://graph.microsoft.com/beta");
        client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var graphClient = new GraphServiceClient(client)
        {
            AuthenticationProvider = new DelegateAuthenticationProvider((requestMessage) => {
                requestMessage.Headers.Authorization = new AuthenticationHeaderValue("bearer", token);
                return Task.CompletedTask;
            }),
            BaseUrl = "https://graph.microsoft.com/beta"
        };
        return graphClient;
    }

    private static string StreamToString(Stream stream)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
