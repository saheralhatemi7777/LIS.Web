using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using مشروع_ادار_المختبرات.DTOS;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class SettingSystemController : Controller
    {

        public SettingSystemController(IHttpClientFactory httpContextFactory)
        {
            _httpClient = httpContextFactory.CreateClient();
        }
        private readonly HttpClient _httpClient;
        public async Task<IActionResult> Index()
        {
            var respnse = await _httpClient.GetAsync($"https://localhost:7116/api/SettingSystem/GetAllSettingData");
            var json = await respnse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<DTOSettingSystem>(json);

            if(result == null)
            {
                TempData["Error"] = "لا يوجد بيانات";
            }

            return View(result);
        }
        [HttpGet]
        public async Task<IActionResult> Create(int id)
        {
            var response = await _httpClient.GetAsync($"https://localhost:7116/api/SettingSystem/GetAllSettingDataByid?id={id}");
            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<DTOSettingSystem>(json);
            if (result != null)
            {
                return View(result);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(DTOSettingSystem dTOSettingSystem)
        {
            if (dTOSettingSystem.ImageFile != null)
            {
                // مسار حفظ الصورة
                var fileName = Path.GetFileName(dTOSettingSystem.ImageFile.FileName);
                var savePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Logo", fileName);

                using (var stream = new FileStream(savePath, FileMode.Create))
                {
                    await dTOSettingSystem.ImageFile.CopyToAsync(stream);
                }

                // خزن اسم الصورة في الخاصية Image
                dTOSettingSystem.Image = "/Logo/" + fileName;
            }

            var json = new StringContent(
                JsonConvert.SerializeObject(dTOSettingSystem),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7116/api/SettingSystem/AddSettingSystem", json);

            if (response.IsSuccessStatusCode)
            {
                TempData["Ok"] = "تمت إضافة الإعدادات بنجاح";
                return RedirectToAction(nameof(Index));
            }

            TempData["Error"] = "حدث خطأ أثناء إضافة البيانات";
            return View(dTOSettingSystem);
        }

    }
}
