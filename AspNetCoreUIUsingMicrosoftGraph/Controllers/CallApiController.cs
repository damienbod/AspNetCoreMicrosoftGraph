using Microsoft.AspNetCore.Mvc;
using Microsoft.Identity.Web;
using System.Threading.Tasks;

namespace GraphApiSharepointIdentity.Controllers
{
    [AuthorizeForScopes(Scopes = new string[] { "api://55fe9feb-e46b-4206-95ee-3a1801233720/access_as_user" })]
    public class CallApiController : Controller
    {
        private readonly ApiService _apiService;
        public CallApiController(ApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IActionResult> Index()
        {
            var dataFromApi = await _apiService.GetApiDataAsync();
            ViewData["Message"] = dataFromApi;
            return View();
        }
    }
}
