using Microsoft.Identity.Web;
using System.Net.Http.Headers;

namespace GraphApiSharepointIdentity;

public class ApiService
{
    private readonly IHttpClientFactory _clientFactory;
    private readonly ITokenAcquisition _tokenAcquisition;
    private readonly IConfiguration _configuration;

    public ApiService(IHttpClientFactory clientFactory,
        ITokenAcquisition tokenAcquisition,
        IConfiguration configuration)
    {
        _clientFactory = clientFactory;
        _tokenAcquisition = tokenAcquisition;
        _configuration = configuration;
    }

    public async Task<string> GetApiDataAsync()
    {
        try
        {
            var client = _clientFactory.CreateClient();

            var scope = _configuration["CallApi:ScopeForAccessToken"];
            var accessToken = await _tokenAcquisition.GetAccessTokenForUserAsync(new[] { scope });

            client.BaseAddress = new Uri(_configuration["CallApi:ApiBaseAddress"]);
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            var response = await client.GetAsync("GraphCalls");
            if (response.IsSuccessStatusCode)
            {
                var responseContent = await response.Content.ReadAsStringAsync();
                var data = $"Graph API user name response: {responseContent}";

                return data;
            }

            throw new ApplicationException($"Status code: {response.StatusCode}, Error: {response.ReasonPhrase}");
        }
        catch (Exception e)
        {
            throw new ApplicationException($"Exception {e}");
        }
    }
}
