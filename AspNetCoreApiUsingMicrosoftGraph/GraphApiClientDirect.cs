using ImageMagick;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace WebApiUsingGraphApi;

public class GraphApiClientDirect
{
    private readonly GraphServiceClient _graphServiceClient;

    // "user.read Sites.Read.All" consented in the App registration
    // The default scope is used because this is a deownstream API OBO
    private const string SCOPES = "https://graph.microsoft.com/.default";

    public GraphApiClientDirect(GraphServiceClient graphServiceClient)
    {
        // https://graph.microsoft.com/.default
        // "user.read Sites.Read.All" consented in the App registration
        _graphServiceClient = graphServiceClient;
    }

    public async Task<User?> GetGraphApiUser()
    {
        return await _graphServiceClient.Me
            .GetAsync(b => b.Options.WithScopes(SCOPES));
    }

    public async Task<string> GetGraphApiProfilePhoto(string oid)
    {
        var photo = string.Empty;
        byte[] photoByte;

        using (var photoStream = await _graphServiceClient
            .Users[oid]
            .Photo
            .Content
            .GetAsync(b => b.Options.WithScopes(SCOPES)))
        {
            photoByte = ((MemoryStream)photoStream!).ToArray();
        }

        using var imageFromFile = new MagickImage(photoByte);
        // Sets the output format to jpeg
        imageFromFile.Format = MagickFormat.Jpeg;
        var size = new MagickGeometry(400, 400);

        // This will resize the image to a fixed size without maintaining the aspect ratio.
        // Normally an image will be resized to fit inside the specified size.
        //size.IgnoreAspectRatio = true;

        imageFromFile.Resize(size);

        // Create byte array that contains a jpeg file
        var data = imageFromFile.ToByteArray();
        photo = Base64UrlEncoder.Encode(data);

        return photo;
    }

    public async Task<string> GetSharepointFile()
    {
        var user = await GetGraphApiUser();

        //var sharepointDomain = "damienbodsharepoint.sharepoint.com";
        //var relativePath = "/sites/listview";
        var fileName = "20210820_130231.jpg";

        // use graph explorer to find site ID
        // There must be a better way...
        var siteId = "damienbodsharepoint.sharepoint.com,73102e3f-af8c-4b6a-b0dd-4afb915cf7de,4d004fec-6241-44cf-86f4-04a8d00cea9e";

        // Graph 5
        var site = await _graphServiceClient.Sites[siteId]
            .GetAsync(b => b.Options.WithScopes(SCOPES));

        var drive = await _graphServiceClient
            .Sites[site!.Id]
            .Drive
            .GetAsync(b => b.Options.WithScopes(SCOPES));

        var driveRoot = await _graphServiceClient.Drives[drive!.Id]
            .Root
            .GetAsync(b => b.Options.WithScopes(SCOPES));

        var items = await _graphServiceClient
           .Drives[drive!.Id]
           .Items[driveRoot!.Id]
           .Children
           .GetAsync(b => b.Options.WithScopes(SCOPES));

        var file = items!.Value!.FirstOrDefault(f => f.Name!.Contains(fileName));

        var stream = await _graphServiceClient
            .Drives[drive.Id]
            .Items[file!.Id].Content
            .GetAsync(b => b.Options.WithScopes(SCOPES));

        var fileAsString = StreamToString(stream!);
        return fileAsString;
    }
    
    private static string StreamToString(Stream stream)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
