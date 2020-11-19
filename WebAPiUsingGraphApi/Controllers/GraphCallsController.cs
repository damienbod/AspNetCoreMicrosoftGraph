using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Identity.Web;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebAPiUsingGraphApi.Controllers
{
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
        [AuthorizeForScopes(ScopeKeySection = "DownstreamApi:Scopes")]
        public async Task<string> Index()
        {
            var user = await _graphApiClientDirect.GetGraphApiUser()
                .ConfigureAwait(false);

            return user.DisplayName;
        }

    }
}
