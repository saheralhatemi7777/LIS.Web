using APiUsers.DTOs;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Collections;
using System.Text;
using مشروع_ادار_المختبرات.DTOS;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class TestCategoryController : Controller
    {
        public TestCategoryController(IHttpClientFactory httpClientFactory)
        {
          _httpClient = httpClientFactory.CreateClient();
        }

        private readonly HttpClient _httpClient;

        [HttpGet]
        public async Task<ActionResult<IEnumerable>> Index(string name)
        {
            if(!string.IsNullOrEmpty(name))
            {

                var responses = await _httpClient.GetAsync($"https://localhost:7116/api/TestCategory/Name?Name={Uri.EscapeDataString(name)}");

                var jsons = await responses.Content.ReadAsStringAsync();
                var contactjsons = JsonConvert.DeserializeObject<DTOTestCategory>(jsons);

                return View(new List<DTOTestCategory> { contactjsons });

            }
            var response = await _httpClient.GetAsync("https://localhost:7116/api/TestCategory");

            if(response == null)
            {
                TempData["Error"]="لا يوجد بيانات لعرضها حاليا";
            }

            var json = await response.Content.ReadAsStringAsync();
            var contactjson = JsonConvert.DeserializeObject<List<DTOTestCategory>>(json);
            return View(contactjson);   
        }

        [HttpGet]
        public async Task<IActionResult> AddNewTestCategory()
        {
            return View();  
        }

        [HttpPost]
        public async Task<IActionResult> AddNewTestCategory(DTOAddCategoryTest dTOTestCategory)
        {
            DTOAddOperations Operations = new DTOAddOperations();
            if (dTOTestCategory == null)
            {
                TempData["Error"]="Error";
            }
            var json = new StringContent(
                JsonConvert.SerializeObject(dTOTestCategory)
                , Encoding.UTF8,
                "application/json"
                );

            //
            Operations.UserId = (int)HttpContext.Session.GetInt32("UserID");
            Operations.RecordId =1;
            Operations.ActionDate = DateTime.Now;
            Operations.TableName = "سجل الفئات";
            Operations.ActionType = $"قام باضافة  بيانات الفئة {dTOTestCategory.CategoryNameEn}";

            var JsonContects = new StringContent(
               JsonConvert.SerializeObject(Operations),
               Encoding.UTF8,
               "application/json");

            var response = await _httpClient.PostAsync("https://localhost:7116/api/TestCategory/AddNewTestCategory", json);
            if(response.IsSuccessStatusCode)
            {
                TempData["OK"]="تم اضافة فئات تحليل جديدة";
                var responses = await _httpClient.PostAsync($"https://localhost:7116/api/Operations/AddOperations", JsonContects);

                return RedirectToAction(nameof(Index));
            }
            return View(dTOTestCategory);
        }

        [HttpGet]
        public async Task<IActionResult> EditTestCategory(int id)
        {
            var response = await _httpClient.GetAsync($"https://localhost:7116/api/TestCategory/GetTestCategoryBy?id={id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "error";
                return RedirectToAction(nameof(Index));
            }

            var json = await response.Content.ReadAsStringAsync();
            var Test = JsonConvert.DeserializeObject<DTOTestCategory>(json);

            var TestCategory = new DTOTestCategory
            {
                CategoryId = Test.CategoryId,
                CategoryNameAr = Test.CategoryNameAr,
                CategoryNameEn = Test.CategoryNameEn,
            };

            return View(TestCategory);
        }

        [HttpPost] 
        public async Task<IActionResult> EditTestCategory(DTOTestCategory categoryTest)
        {
            DTOAddOperations Operations = new DTOAddOperations();

            var JsonContact = new StringContent(JsonConvert.SerializeObject(categoryTest), Encoding.UTF8, "application/json");

            //
            Operations.UserId = (int)HttpContext.Session.GetInt32("UserID");
            Operations.RecordId = 1;
            Operations.ActionDate = DateTime.Now;
            Operations.TableName = "سجل الفئات";
            Operations.ActionType = $"قام بتعديل  بيانات الفئة {categoryTest.CategoryNameEn}";

            var JsonContects = new StringContent(
               JsonConvert.SerializeObject(Operations),
               Encoding.UTF8,
               "application/json");
            var response = await _httpClient.PutAsync($"https://localhost:7116/api/TestCategory/EditCategory?id={categoryTest.CategoryId}", JsonContact);

            if (response.IsSuccessStatusCode)
            {

                TempData["Successful"] = "تم تعديل بيانات الفئة بنجاح";
                var responses = await _httpClient.PostAsync($"https://localhost:7116/api/Operations/AddOperations", JsonContects);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"حدث خطأ أثناء تعديل الفئة: {errorResponse}";
                return RedirectToAction(nameof(Index));
            }
        }

       
        //koestpdf تقارير دوت نت فريم  وورك

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteTestCategory(int id)
        {
            DTOAddOperations Operations = new DTOAddOperations();
            //
            Operations.UserId = (int)HttpContext.Session.GetInt32("UserID");
            Operations.RecordId = 1;
            Operations.ActionDate = DateTime.Now;
            Operations.TableName = "سجل الفئات";
            Operations.ActionType = $"قام بحذف  بيانات الفئة رقم {id} ";

            var JsonContects = new StringContent(
               JsonConvert.SerializeObject(Operations),
               Encoding.UTF8,
               "application/json");

            var result = await _httpClient.DeleteAsync($"https://localhost:7116/api/TestCategory?id={id}");
            if (!result.IsSuccessStatusCode)
            {
                var errorResponse = await result.Content.ReadAsStringAsync();
                TempData["Error"] = $"حدث خطأ أثناء حذف الفئة: {errorResponse}";
                return RedirectToAction(nameof(Index));
            }
            var responses = await _httpClient.PostAsync($"https://localhost:7116/api/Operations/AddOperations", JsonContects);

            TempData["OK"]="تم حذف بيانات الفئة بنجاح";
            return RedirectToAction(nameof(Index));

        }

        
        
    }
}
