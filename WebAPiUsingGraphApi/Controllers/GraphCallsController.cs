using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Threading.Tasks;

namespace WebApiUsingGraphApi.Controllers
{
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
        public async Task<string> Index()
        {
            var user = await _graphApiClientDirect.GetGraphApiUser()
                .ConfigureAwait(false);

            return user.DisplayName;
        }

    }
}
