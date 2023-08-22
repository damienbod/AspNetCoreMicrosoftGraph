using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApiUsingGraphApi.Controllers;

[Authorize]
[ApiController]
[Route("[controller]")]
public class GraphCallsController : ControllerBase
{
    private readonly GraphApiClientDirect _graphApiClientDirect;

    public GraphCallsController(GraphApiClientDirect graphApiClientDirect)
    {
        _graphApiClientDirect = graphApiClientDirect;
    }

    [HttpGet]
    public async Task<string?> Get()
    {
        var user = await _graphApiClientDirect.GetGraphApiUser();

        // var photo = await _graphApiClientDirect.GetGraphApiProfilePhoto();
        var file = await _graphApiClientDirect.GetSharepointFile();
        return user!.DisplayName;
    }
}
