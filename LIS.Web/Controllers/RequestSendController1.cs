using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Drawing;
using System.Drawing.Imaging;
using System.Text;
using System.Text.Json;
using ZXing;
using ZXing.Common;
using مشروع_ادار_المختبرات.DTOS;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using مشروع_ادار_المختبرات.Helpers;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class RequestSendController : Controller
    {

        public RequestSendController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }
        private readonly HttpClient _httpClient;
        public async Task<IActionResult> Index()
        {
            var requests = await _httpClient.GetFromJsonAsync<List<RequestViewDto>>(
                "https://localhost:7116/api/RequestTest/GetAllDataRequestTest"
            );

            // عرض البيانات في الفيو
            return View(requests);
        }
        int _requestId;

        public async Task<IActionResult> ShowRequestData(int id)
        {

            var requests = await _httpClient.GetAsync(
                           $"https://localhost:7116/api/RequestTest/GetAllDataRequestTestbyid?id={id}");
            var json = await requests.Content.ReadAsStringAsync();

            var requestss = System.Text.Json.JsonSerializer.Deserialize<List<RequestTestDto>>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            var patientName = requestss.First().PatientName;

            // مجلد التخزين الفعلي
            string folderPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Barcodes");

            //BarcodeHelper توليد الباركود وحفظه باسم المريض يجلب داله التوليد من الكلاس المساعد في المشروع
            string savedPath = BarcodeHelper.GenerateAndSaveQrCode(patientName, folderPath, patientName + ".png");

            // تمرير المسار للـ View
            ViewBag.BarcodePath = "/Barcodes/" + Path.GetFileName(savedPath);
            ViewBag.Request = requestss.First();
            return View(requestss);
        }


        [HttpGet]
        public async Task<IActionResult> SendRequest(int requestId)
        {
            var status = "قيد التحليل";

            var jsonContent = new StringContent(
                JsonConvert.SerializeObject(status),
                Encoding.UTF8,
                "application/json"
            );

            var response = await _httpClient.PutAsync(
                $"https://localhost:7116/api/Recuests/EditRecuestsStatus?id={requestId}",
                jsonContent
            );

            if (response.IsSuccessStatusCode)
            {
                TempData["OK"] = "تم ارسال الطلب للمختبر";
            }
            else
            {
                TempData["Error"] = "لم يتم ارسال الطلب للمختبر";
            }

            return RedirectToAction(nameof(Index));
        }

        public IActionResult PrintQr(string name)
        {
            // جلب بيانات الطلب (مثل اسم المريض)

           
            // مسار الباركود المحفوظ مسبقًا
            string barcodePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/Barcodes", name + ".png");

            if (!System.IO.File.Exists(barcodePath))
                return NotFound("BarCode image not found");

            // إنشاء PDF في الذاكرة
            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
                var pdf = new PdfDocument(writer);
                var document = new Document(pdf);

                // إضافة صورة الباركود
                var imageData = iText.IO.Image.ImageDataFactory.Create(barcodePath);
                var pdfImage = new iText.Layout.Element.Image(imageData);
                pdfImage.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
                pdfImage.ScaleToFit(250, 250);

                document.Add(pdfImage);

                document.Close(); // هذا يغلق الـ PdfDocument ويغلق writer

                // الحل: نسخ المصفوفة قبل إعادة الملف
                var fileBytes = ms.ToArray();

                return File(fileBytes, "application/pdf");
            }

        }
    }
}

