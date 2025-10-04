using Microsoft.AspNetCore.Mvc;
using مشروع_ادار_المختبرات.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Newtonsoft.Json;
using System.Text;
using مشروع_ادار_المختبرات.DTOS;
using System.Net.Http;
using System.Collections.Generic;
using System;
using Microsoft.AspNetCore.Mvc.Rendering;
using APiUsers.DTOs;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class UsersController : Controller
    {
        private readonly HttpClient _httpClient;

        public UsersController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        private readonly string _apiBaseUrl = "https://localhost:7116/api";

        [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
        [Authorize] 
        public async Task<IActionResult> Index(string searchTerm)
        {
            string url;

            if (!string.IsNullOrEmpty(searchTerm))
            {

                url = $"{_apiBaseUrl}/Users/Name?Name={Uri.EscapeDataString(searchTerm)}";

            }
            else
            {
                url = $"{_apiBaseUrl}/Users";
            }

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(searchTerm))
                {

                    var user = JsonConvert.DeserializeObject<DTOUsers>(json);
                    return View(new List<DTOUsers> { user }); 

                }
                else
                {
                    var users = JsonConvert.DeserializeObject<List<DTOUsers>>(json);
                    return View(users);
                }
            }

            ViewBag.Error = "فشل تحميل البيانات من API.";
            return View(new List<DTOUsers>());
        }


        [HttpGet]
        public async Task<IActionResult> Create()
        {
           await LoadRoelData();
            return View();
        }


        [HttpPost()]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DTOEditUsers user)
        {
            DTOAddOperations Operations = new DTOAddOperations();
            //
            Operations.UserId = (int)HttpContext.Session.GetInt32("UserID");
            Operations.RecordId = 1;
            Operations.ActionDate = DateTime.Now;
            Operations.TableName = "سجل المستخدمين";
            Operations.ActionType = $"قام باضافة  بيانات المستخدم {user.FullName}";

            var JsonContects = new StringContent(
               JsonConvert.SerializeObject(Operations),
               Encoding.UTF8,
               "application/json");
            await LoadRoelData();

            if (!ModelState.IsValid)
                return View(user);

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(user),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync("https://localhost:7116/api/Users/AddUsers", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["Successful"] = "تم اضافة بيانات المستخدم بنجاح";
                var responses = await _httpClient.PostAsync($"https://localhost:7116/api/Operations/AddOperations", JsonContects);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"{errorResponse}";
                return View(user);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            await LoadRoelData();
            var response = await _httpClient.GetAsync($"https://localhost:7116/api/Users/byiD?iD={id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "المستخدم غير موجود";
                return NotFound();
            }
            var json = await response.Content.ReadAsStringAsync();
            var user = JsonConvert.DeserializeObject<DTOEditUsers>(json);

            var dtoUser = new DTOEditUsers
            {
                UserID = user.UserID,
                FullName = user.FullName,
                Password = user.Password,
                Email = user.Email,
                RoleId = user.RoleId,
                IsActive = user.IsActive
            };

            return View(dtoUser);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(DTOEditUsers user)
        {
            DTOAddOperations Operations = new DTOAddOperations();
            //
            Operations.UserId = (int)HttpContext.Session.GetInt32("UserID");
            Operations.RecordId = 1;
            Operations.ActionDate = DateTime.Now;
            Operations.TableName = "سجل المستخدمين";
            Operations.ActionType = $"قام بتعديل  بيانات المستخدم {user.FullName}";

            var JsonContects = new StringContent(
               JsonConvert.SerializeObject(Operations),
               Encoding.UTF8,
               "application/json");
            if (!ModelState.IsValid)
                return View(user);

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(user),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PutAsync($"https://localhost:7116/api/Users/UpdateUser?id={user.UserID}",jsonContent);

            if (response.IsSuccessStatusCode)
            {
                var responseData = await response.Content.ReadAsStringAsync();

                TempData["Successful"] = "تم تعديل بيانات المستخدم بنجاح";
                var responses = await _httpClient.PostAsync($"https://localhost:7116/api/Operations/AddOperations", JsonContects);

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"حدث خطأ أثناء تعديل المستخدم: {errorResponse}";

                return View(user);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            DTOAddOperations Operations = new DTOAddOperations();
            //
            Operations.UserId = (int)HttpContext.Session.GetInt32("UserID");
            Operations.RecordId = 1;
            Operations.ActionDate = DateTime.Now;
            Operations.TableName = "سجل المستخدمين";
            Operations.ActionType = $"قام بحذف  بيانات المستخدم رقم {id}";

            var JsonContects = new StringContent(
               JsonConvert.SerializeObject(Operations),
               Encoding.UTF8,
               "application/json");
            var response = await _httpClient.DeleteAsync($"https://localhost:7116/api/Users/Delete?id={id}");

            if (response.IsSuccessStatusCode)
            {     
                TempData["Successful"] = "تم حذف المستخدم بنجاح.";
                var responses = await _httpClient.PostAsync($"https://localhost:7116/api/Operations/AddOperations", JsonContects);

            }

            else
            {
                ///var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"حدث خطأ أثناء الحذف: المستخدم مرتبط";
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult Dashbord()
        {
            return View();
        }

        public IActionResult MainScreen()
        {
            return View();
        }

        public async Task LoadRoelData()
        {
            var response = await _httpClient.GetAsync($"https://localhost:7116/api/Role/GetAllData");
            if(response.IsSuccessStatusCode)
            {
                var RoleJson = await response.Content.ReadAsStringAsync();
                var RoleList = JsonConvert.DeserializeObject<List<DTORoles>>(RoleJson);
                ViewBag.PatientsList = new SelectList(RoleList.OrderBy(p => p.Name), "RoleId", "Name");
            }   
            else
            {
                TempData["Error"]="لا يوجد بيانات";
            }
        }

    }
}
