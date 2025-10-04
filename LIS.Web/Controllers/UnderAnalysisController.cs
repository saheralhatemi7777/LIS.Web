using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using مشروع_ادار_المختبرات.DTOS;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class UnderAnalysisController : Controller
    {

        public UnderAnalysisController(IHttpClientFactory httpClientFactory)
        {
            _httpClien = httpClientFactory.CreateClient();
        }
        private readonly HttpClient _httpClien;
        public async Task<IActionResult> Index()
        {
            var response = await _httpClien.GetFromJsonAsync<List<RequestViewDto>>($"https://localhost:7116/api/RequestTest");
            if(response == null)
            {
                TempData["Error"] = "لا يوجد بيانات";
            }

            return View(response);
        }

    }
}
