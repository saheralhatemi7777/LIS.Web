using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using System.Web;
using مشروع_ادار_المختبرات.DTOS;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class TestResultsController : Controller
    {
        public TestResultsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }
        private readonly HttpClient _httpClient;
        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> AddTestResults(int id)
        {
            var response = await _httpClient.GetFromJsonAsync<List<RequestViewDto>>($"https://localhost:7116/api/RequestTest/GetByid?id={id}");
            if (response == null || !response.Any())
            {
                TempData["Error"] = "لا توجد بيانات لهذا الطلب";
                return RedirectToAction("Index", "UnderAnalysis");
            }

            return View(response);

        }

        [HttpPost]
        public async Task<IActionResult> SaveResultAsync(SaveTestResultRequestDto dto)
        {
              if (dto == null || dto.TestId == null || !dto.TestId.Any())
             {
               return Content("<script>alert('لا توجد بيانات لإرسالها'); window.history.back();</script>", "text/html");
             }
            dto.LabTechniciansUserID= (int)HttpContext.Session.GetInt32("UserID");
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json,
            Encoding.UTF8,
            "application/json"
            );

            var response = await _httpClient.PostAsync("https://localhost:7116/api/TestResult/AddNewTestResults", content);
            var status = "تم اصدار التحليل✅";

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(status),
                Encoding.UTF8,
                "application/json");

            var responses = await _httpClient.PutAsync(
                $"https://localhost:7116/api/Recuests/EditRecuestsStatus?id={dto.Requestid}",
                jsonContent
            );

            if (response.IsSuccessStatusCode)
            {
                
                var responseString = await response.Content.ReadAsStringAsync();
              
                return RedirectToAction("Index","UnderAnalysis");

            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"حدث خطأ أثناء إضافة التحليل: {errorResponse}";
                ModelState.AddModelError(string.Empty, "فشل إرسال البيانات إلى API.");
                return RedirectToAction("Index", "UnderAnalysis");
            }

        }


    }
}
