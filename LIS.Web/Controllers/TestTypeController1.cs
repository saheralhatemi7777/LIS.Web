using APiUsers.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Newtonsoft.Json;
using System.Text;
using مشروع_ادار_المختبرات.DTOS;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class TestsTypeController : Controller
    {
        public TestsTypeController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        private readonly HttpClient _httpClient;

        [HttpGet]
        public async Task<IActionResult> Index(string name)
        {
            if(!string.IsNullOrEmpty(name))
            {
                var responses = await _httpClient.GetAsync($"https://localhost:7116/api/Test/GetTestByName?Name={name}");
                if (responses == null)
                {
                    TempData["Error"]="لا يوجد بيانات بهذ الاسم";
                }
                else
                {
                    var jsons = await responses.Content.ReadAsStringAsync();
                    if(jsons == null)
                    {
                        TempData["Error"]="لا يوجد بيانات بهذ الاسم";
                    }
                    var JsonContacts = JsonConvert.DeserializeObject<DTOTestsType>(jsons);
                    return View(new List<DTOTestsType> { JsonContacts });
                }
            }
            var response = await _httpClient.GetAsync("https://localhost:7116/api/Test/test");
            if(!response.IsSuccessStatusCode)
            {
                TempData["Error"]="لا يوجد استجابة بيانات";
            }

            var json  = await response.Content.ReadAsStringAsync();
            var JsonContact = JsonConvert.DeserializeObject<List<DTOTestsType>>(json);
            return View(JsonContact);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
         {
            await LoadTestCategoryData();
            return View();  
         }

        [HttpPost]
        public async Task<IActionResult> Create(DTOAddTestTypes dTO)
        {
            DTOAddOperations Operations = new DTOAddOperations();
            var jsonContact =new StringContent(
                JsonConvert.SerializeObject(dTO)
                ,Encoding.UTF8,
                "application/json");

            //
            Operations.UserId = (int)HttpContext.Session.GetInt32("UserID");
            Operations.RecordId = 1;
            Operations.ActionDate = DateTime.Now;
            Operations.TableName = "سجل التحاليل";
            Operations.ActionType = $"قام باضافة  التحليل  {dTO.TestNameEn}";

            var JsonContects = new StringContent(
               JsonConvert.SerializeObject(Operations),
               Encoding.UTF8,
               "application/json");
            var response = await _httpClient.PostAsync($"https://localhost:7116/api/Test/AddTestType", jsonContact);
            if(!response.IsSuccessStatusCode)
            {
                TempData["Error"]="هناك خطاء في الاضافة";
            }
            var responses = await _httpClient.PostAsync($"https://localhost:7116/api/Operations/AddOperations", JsonContects);

            TempData["Ok"]="تم اضافة بيانات بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {

            DTOAddOperations Operations = new DTOAddOperations();
            //
            Operations.UserId = (int)HttpContext.Session.GetInt32("UserID");
            Operations.RecordId = 1;
            Operations.ActionDate = DateTime.Now;
            Operations.TableName = "سجل التحاليل";
            Operations.ActionType = $"قام بحذف  بيانات التحليل رقم {id}";

            var JsonContects = new StringContent(
               JsonConvert.SerializeObject(Operations),
               Encoding.UTF8,
               "application/json");

            var response = await _httpClient.DeleteAsync($"https://localhost:7116/api/Test/DeleteTest?id={id}");
            if(!response.IsSuccessStatusCode)
            {
                TempData["Error"]="حدث خطاء اثناء الحذف";
            }
            var responses = await _httpClient.PostAsync($"https://localhost:7116/api/Operations/AddOperations", JsonContects);
            TempData["OK"]="تم الحذف";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            await LoadTestCategoryData();

            var response = await _httpClient.GetAsync($"https://localhost:7116/api/Test/GetTestById?id={id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "خطاء في التعديل";
            }

            var json = await response.Content.ReadAsStringAsync();
            var test = JsonConvert.DeserializeObject<DTOAddTestTypes>(json);

            return View(test);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DTOAddTestTypes dto)
        {
            DTOAddOperations Operations = new DTOAddOperations();
            await LoadTestCategoryData();

            var json = new StringContent(JsonConvert.SerializeObject(dto), Encoding.UTF8, "application/json");
            var response = await _httpClient.PutAsync($"https://localhost:7116/api/Test/EditTest?id={dto.TestId}", json);
            //
            Operations.UserId = (int)HttpContext.Session.GetInt32("UserID");
            Operations.RecordId =1;
            Operations.ActionDate = DateTime.Now;
            Operations.TableName = " سجل التحاليل";
            Operations.ActionType = $" قام بتعديل  بيانات التحليل {dto.TestNameEn}";

            var JsonContects = new StringContent(
               JsonConvert.SerializeObject(Operations),
               Encoding.UTF8,
               "application/json");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "لا يوجد بيانات";
                return View(dto); // رجّع نفس الموديل للعرض مرة ثانية
            }
            var responses = await _httpClient.PostAsync($"https://localhost:7116/api/Operations/AddOperations", JsonContects);

            TempData["OK"] = "تم تعديل البيانات بنجاح";
            return RedirectToAction(nameof(Index));
        }

        public async Task LoadTestCategoryData()
        {
            var response = await _httpClient.GetAsync("https://localhost:7116/api/TestCategory");
            if(response.IsSuccessStatusCode)
            {
                var RoleJson = await response.Content.ReadAsStringAsync();
                var RoleList = JsonConvert.DeserializeObject<List<DTOTestCategory>>(RoleJson);
                ViewBag.PatientsList = new SelectList(RoleList.OrderBy(p => p.CategoryNameEn), "CategoryId", "CategoryNameEn");
            }

        }

    }
}
