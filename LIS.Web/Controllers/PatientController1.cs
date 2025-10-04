using Microsoft.AspNetCore.Mvc;
using مشروع_ادار_المختبرات.Models;
using System;
using System.Linq;
using Newtonsoft.Json;
using System.Text;
using مشروع_ادار_المختبرات.DTOS;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using APiUsers.DTOs;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class PatientController : Controller
    {
        private readonly HttpClient _httpClient;

        public PatientController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }
       

        private readonly string _apiBaseUrl = "https://localhost:7116/api";
        public async Task<IActionResult> Index(string searchTerm)
        {
            string url;

            if (!string.IsNullOrEmpty(searchTerm))
            {

                url = $"{_apiBaseUrl}/Patient/Samples/Name?Name={Uri.EscapeDataString(searchTerm)}";

            }
            else
            {
                url = $"{_apiBaseUrl}/Patient";
            }

            var response = await _httpClient.GetAsync(url);

            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync();

                if (!string.IsNullOrEmpty(searchTerm))
                {

                    var patents = JsonConvert.DeserializeObject<List<DTOPatients>>(json);
                    return View( patents );

                }
                else
                {
                    var patient = JsonConvert.DeserializeObject<List<DTOPatients>>(json);
                    return View(patient);
                }
            }

            ViewBag.Error = "فشل تحميل البيانات من API.";
            return View(new List<DTOPatients>());
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]

       
        public async Task<IActionResult> Create(Patient patient)
        {
            if (!ModelState.IsValid)
                return View(patient);

            var supervisorId = HttpContext.Session.GetInt32("UserID");
            if (supervisorId == null)
            {
                TempData["Error"] = "المشرف غير مسجل. الرجاء تسجيل الدخول أولاً.";
                return RedirectToAction("Login", "Account"); 
            }

            // توليد كلمة مرور فريدة
            patient.Password = await GenerateUniquePasswordAsync();

            // تعيين SupervisorID
            patient.SupervisorID = supervisorId.Value;

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(patient),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PostAsync($"{_apiBaseUrl}/Patient/Addpatients", jsonContent);

            if (response.IsSuccessStatusCode)
            {
                TempData["Successful"] = "تم اضافة بيانات المريض بنجاح";
                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["ErrorScript"] = $@"<script>
                         alert('حدث خطأ أثناء اضافة المريض: {errorResponse}');
                         </script>";
                return View(patient);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var response = await _httpClient.GetAsync($"https://localhost:7116/api/Patient/Get?id={id}");
            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "المستخدم غير موجود";
                return NotFound();
            }
            var json = await response.Content.ReadAsStringAsync();
            var patinet = JsonConvert.DeserializeObject<Patient>(json);

            var dtoPatients = new DTOPatients
            {
                PatientID = patinet.PatientID,
                FullName = patinet.FullName,
                BirthDate = patinet.BirthDate,
                Gender = patinet.Gender,
                phoneNumber = patinet.phoneNumber,
                Password = patinet.Password,
                Address = patinet.Address,
                SupervisorID = HttpContext.Session.GetInt32("UserID"),
                UserName = "",
            };
             return View(dtoPatients);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(DTOPatients patient)
        {

            DTOAddOperations Operations = new  DTOAddOperations();
            if (!ModelState.IsValid)
             return View(patient);

            patient.SupervisorID = (int)HttpContext.Session.GetInt32("UserID");
            var JsonContect = new StringContent(
                JsonConvert.SerializeObject(patient),
                Encoding.UTF8,
                "application/json");
            //
            Operations.UserId =(int)HttpContext.Session.GetInt32("UserID");
            Operations.RecordId = patient.PatientID;
            Operations.ActionDate = DateTime.Now;
            Operations.TableName = "سجل المرضى";
            Operations.ActionType = $"قام بتعديل  بيانات المريض{patient.FullName}";

            var JsonContects = new StringContent(
               JsonConvert.SerializeObject(Operations),
               Encoding.UTF8,
               "application/json");

            var response = await _httpClient.PutAsync($"https://localhost:7116/api/Patient/patientid?patientid={patient.PatientID}",JsonContect);
            
            if(response.IsSuccessStatusCode)
            {
                //اضافة بيانات العمليه
                var responses = await _httpClient.PostAsync($"https://localhost:7116/api/Operations/AddOperations", JsonContects);
                var responseData = await response.Content.ReadAsStringAsync();

                TempData["Successful"] = "تم تعديل بيانات المستخدم بنجاح";

                return RedirectToAction(nameof(Index));
            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"حدث خطأ أثناء تعديل المستخدم: {errorResponse}";

                return View(patient);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            var response = await _httpClient.DeleteAsync($"https://localhost:7116/api/Patient/by?id={id}");
            if(response.IsSuccessStatusCode)
            {
                TempData["Successful"] = "تم حذف المريض بنجاح";
            }
            else
            {
                //var error = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"لا يسمح بحذف بيانات المريض المرتبط";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task<string> GenerateUniquePasswordAsync()
        {
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Patient/GetAll"); // تأكد من رابط API
            if (!response.IsSuccessStatusCode)
                return new Random().Next(100000000, 999999999).ToString(); // في حالة فشل الاتصال، توليد رقم عشوائي

            var content = await response.Content.ReadAsStringAsync();
            var existingPatients = JsonConvert.DeserializeObject<List<Patient>>(content);

            var usedPasswords = existingPatients.Select(p => p.Password).ToHashSet();

            var rnd = new Random();
            string newPassword;
            do
            {
                newPassword = rnd.Next(100000000, 999999999).ToString(); // رقم من 9 خانات
            } while (usedPasswords.Contains(newPassword));

            return newPassword;
        }

        [HttpGet]
        public async Task<IActionResult> AddPatients()
        {
           
                return View(new DTOPatients());
            
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddPatients(string Name)
        {
            if (string.IsNullOrWhiteSpace(Name))
            {
                TempData["Error"] = "يرجى إدخال اسم المريض للبحث.";
                return View();
            }

            var response = await _httpClient.GetAsync(
                $"https://localhost:7116/api/Patient/Samples/Name?Name={Name}");

            if (!response.IsSuccessStatusCode)
            {
                TempData["Error"] = "المريض غير موجود.";
                return View();
            }

            var json = await response.Content.ReadAsStringAsync();
            var patient = JsonConvert.DeserializeObject<Patient>(json);

            if (patient == null)
            {
                TempData["Error"] = "المريض غير موجود.";
                return View();
            }
            HttpContext.Session.SetInt32("SelectedPatientID", patient.PatientID);

            var dtoPatients = new DTOPatients
            {
                PatientID = patient.PatientID,
                FullName = patient.FullName,
                BirthDate = patient.BirthDate,
                Gender = patient.Gender,
                phoneNumber = patient.phoneNumber,
                Password = patient.Password,
                Address = patient.Address,
                SupervisorID = HttpContext.Session.GetInt32("UserID"),
                UserName = ""
            };

            return View(dtoPatients);
        }

      
    }
}
