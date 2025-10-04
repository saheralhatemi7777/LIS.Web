using Microsoft.AspNetCore.Mvc;
using مشروع_ادار_المختبرات.DTOS;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class OperationsController : Controller
    {

        public OperationsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }
        private readonly HttpClient _httpClient;
        public async Task<IActionResult> Index(string searchTerm)
        {
            if (!string.IsNullOrEmpty(searchTerm))
            {

                var responsebyname = await _httpClient.GetFromJsonAsync<List<DTOShowOperations>>($"https://localhost:7116/api/Operations/GetAllOperationsbyname?name=mohammed%20");
                return View(responsebyname);

            }

            var response = await _httpClient.GetFromJsonAsync<List<DTOShowOperations>>("https://localhost:7116/api/Operations/GetAllOperations");
                if (response == null)
                {
                    TempData["ERROR"] = " لا يوجد بيانات";
                }
                return View(response);
            
        }
    }
}
