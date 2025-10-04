using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using مشروع_ادار_المختبرات.Models;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using ZXing;
using ZXing.Common;
using System.Drawing;
using System.Drawing.Imaging;
using Newtonsoft.Json;
using مشروع_ادار_المختبرات.DTOS;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using System.Collections.Generic;
using System.Net.Http;
using DocumentFormat.OpenXml;
using Microsoft.AspNetCore.Http;
using APiUsers.DTOs;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class RequestController : Controller
    {
        private readonly HttpClient _httpClient;
        public int _Sampleid = 0;

        public RequestController( IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }
        private readonly string _apiBaseUrl = "https://localhost:7116/api";

        int _PatientId;
        public IActionResult SelectPatient(int patientId, string Name)
        {
            HttpContext.Session.SetInt32("SelectedPatientID", patientId);
            HttpContext.Session.SetString("PatientName", Name);
            return RedirectToAction("Create");
        }

        public async Task<IActionResult> Index()
        {
            var requests = await _httpClient.GetFromJsonAsync<List<RequestViewDto>>(
               "https://localhost:7116/api/RequestTest/GetAllDataRequestTest"
           );

            // عرض البيانات في الفيو
            return View(requests);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DTORecuests Recuest)
         {

            if (!ModelState.IsValid)
            {
                return View(Recuest);
            }

            int? userId = HttpContext.Session.GetInt32("UserID");
            if (userId == null)
            {
                TempData["Error"] = "خطأ: لم يتم العثور على رقم المستخدم في الجلسة.";
                return View(Recuest);
            }


            var patientId = HttpContext.Session.GetInt32("SelectedPatientID");
            if (patientId == null)
            {
                TempData["Error"] = "لم يتم اختيار مريض.";
                return RedirectToAction("AddPatients");
            }

                _PatientId = patientId.Value; 
                Recuest.UserID = userId.Value;
                Recuest.PatientID =patientId.Value;
                Recuest.CreatedAt = DateTime.Now;

                var json = JsonConvert.SerializeObject(Recuest);
                var content = new StringContent(json,
                Encoding.UTF8,
                "application/json"
                );

            var response = await _httpClient.PostAsync("https://localhost:7116/api/Recuests/AddNewRecuests", content);

            if (response.IsSuccessStatusCode)
            {
                var responseString = await response.Content.ReadAsStringAsync();
                _Sampleid = int.Parse(responseString);
                HttpContext.Session.SetInt32("NewSampleID", _Sampleid);
                TempData["NewSampleID"] = _Sampleid;

                HttpContext.Session.SetInt32("NewSampleID", _Sampleid);

                TempData["NewSampleID"] = _Sampleid;

                return RedirectToAction("Index", "TestTypes");

            }
            else
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"حدث خطأ أثناء إضافة التحليل: {errorResponse}";
                ModelState.AddModelError(string.Empty, "فشل إرسال البيانات إلى API.");
                return View(Recuest);
            }


        }


        public async Task<IActionResult> CanselRequest(int id,int id2)
        {
            DTOAddOperations Operations = new DTOAddOperations();
            //
            Operations.UserId = (int)HttpContext.Session.GetInt32("UserID");
            Operations.RecordId = 1;
            Operations.ActionDate = DateTime.Now;
            Operations.TableName = "سجل الطلبات";
            Operations.ActionType = $"قام بالغاء  الطلب رقم {id2}";

            var JsonContects = new StringContent(
               JsonConvert.SerializeObject(Operations),
               Encoding.UTF8,
               "application/json");
            //كود لالغاء الطلب حذف بيانات الطلب مع التحاليل
            var response1 = await _httpClient.DeleteAsync($"https://localhost:7116/api/RequestTest?id={id2}");//حذف الطلب
            var response2 = await _httpClient.DeleteAsync($"https://localhost:7116/api/Recuests/DeleteRecuests?id={id}");//حذف التحليل
            var responses = await _httpClient.PostAsync($"https://localhost:7116/api/Operations/AddOperations", JsonContects);//بيانات العمليه

            return RedirectToAction(nameof(Index));
        }

     
    }
}
