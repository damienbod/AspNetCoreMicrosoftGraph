using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace GraphApiSharepointIdentity.Controllers
{
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
