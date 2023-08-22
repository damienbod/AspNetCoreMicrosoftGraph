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

    public GraphApiClientDirect(GraphServiceClient graphServiceClient)
    {
        _graphServiceClient = graphServiceClient;
    }

    public async Task<User?> GetGraphApiUser()
    {
        return await _graphServiceClient.Me
            .GetAsync(b => b.Options.WithScopes("User.ReadBasic.All", "user.read"));
    }

    /// <summary>
    /// https://learn.microsoft.com/en-us/azure/active-directory/verifiable-credentials/how-to-use-quickstart-verifiedemployee
    /// UrlEncode(Base64Encode(photo)) format. To use the photo, 
    /// the verifier application has to 
    /// Base64Decode(UrlDecode(photo)).
    public async Task<string> GetGraphApiProfilePhoto(string oid)
    {
        var photo = string.Empty;
        byte[] photoByte;

        using (var photoStream = await _graphServiceClient.Users[oid].Photo
            .Content.GetAsync())
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

        if (user == null)
            throw new ArgumentException($"User not found in AD.");

        var sharepointDomain = "damienbodsharepoint.sharepoint.com";
        var relativePath = "/sites/listview";
        var fileName = "20210820_130231.jpg";

        // "AllSites.Read"
        var site = await _graphServiceClient
            .Sites[sharepointDomain]
            .SiteWithPath(relativePath)
            .Request()
            .GetAsync();

        var drive = await _graphServiceClient
            .Sites[site.Id]
            .Drive
            .Request()
            .GetAsync();

        var items = await _graphServiceClient
            .Sites[site.Id]
            .Drives[drive.Id]
            .Root
            .Children
            .Request()
            .GetAsync();

        var file = items.FirstOrDefault(f => f.File != null && f.WebUrl.Contains(fileName));

        var stream = await _graphServiceClient
            .Sites[site.Id]
            .Drives[drive.Id]
            .Items[file!.Id].Content
            .Request()
            .GetAsync();

        var fileAsString = StreamToString(stream);
        return fileAsString;
    }

    private static string StreamToString(Stream stream)
    {
        stream.Position = 0;
        using var reader = new StreamReader(stream, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
