using APiUsers.DTOs.DTOSTests;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using مشروع_ادار_المختبرات.DTOS;
using مشروع_ادار_المختبرات.Models;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using System.Net.Http.Json;
using iText.IO.Font;
using iText.Kernel.Pdf.Canvas.Draw;
using Microsoft.AspNetCore.Http;
using System.Text.Json;
using iText.Layout.Borders;
using iText.IO.Image;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class TestTypesController : Controller
    {

        public TestTypesController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        private readonly HttpClient _httpClient;


        public async Task<IActionResult> Index(string name)
        {

            if (!string.IsNullOrEmpty(name))
            {
                var responses = await _httpClient.GetAsync($"https://localhost:7116/api/Test/GetTestByName?Name={name}");
                if (responses == null)
                {
                    TempData["Error"] = "لا يوجد بيانات بهذ الاسم";
                }
                else
                {
                    var jsons = await responses.Content.ReadAsStringAsync();
                    if (jsons == null)
                    {
                        TempData["Error"] = "لا يوجد بيانات بهذ الاسم";
                    }
                    var JsonContacts = JsonConvert.DeserializeObject<DTOTest>(jsons);
                    return View(new List<DTOTest> { JsonContacts });
                }
            }

            int sampleId = 0;
            if (TempData["NewSampleID"] != null)
            {
                sampleId = (int)TempData["NewSampleID"];
                ViewBag.NewSampleID = sampleId;
                TempData["OK"] = sampleId;
            }
            var respnse = await _httpClient.GetAsync($"https://localhost:7116/api/Test/test");
            if (respnse != null)
            {
                TempData["OK"] = sampleId;
                var jsonconect = await respnse.Content.ReadAsStringAsync();
                var test = JsonConvert.DeserializeObject<List<DTOTest>>(jsonconect);
                return View(test);
            }
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> CreateTest(List<int> selectedTestIds)
        {
            var sampleId = HttpContext.Session.GetInt32("NewSampleID");
            if (sampleId == null)
            {
                TempData["Error"] = "لم يتم العثور على رقم التحليل في الجلسة.";
                return RedirectToAction("AddPatients");
            }

            if (selectedTestIds == null || !selectedTestIds.Any())
            {
                TempData["Error"] = "لم يتم اختيار أي تحليل.";
                return RedirectToAction("Index");
            }

            // إرسال التحاليل إلى API
            var payload = new DTORequestTest
            {
                RequestID = sampleId.Value,
                TestIds = selectedTestIds
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync("https://localhost:7116/api/RequestTest", content);

            if (!response.IsSuccessStatusCode)
            {
                var errorResponse = await response.Content.ReadAsStringAsync();
                TempData["Error"] = $"حدث خطأ أثناء إضافة التحاليل: {errorResponse}";
                return RedirectToAction("Index");
            }

            var addedIdsJson = await response.Content.ReadAsStringAsync();
            HttpContext.Session.SetString("SeleceRequestTestiDs", addedIdsJson);

            // ارسال بيانات المريض للAPI للتحقق من وجود سجل أو إضافته
            int patientId = (int)HttpContext.Session.GetInt32("SelectedPatientID");
            int technicianId = (int)HttpContext.Session.GetInt32("UserID");

            // 1- استعلام API للتأكد إذا السجل موجود
            var checkRecordResponse = await _httpClient.GetAsync($"https://localhost:7116/api/RecordPatients/GOToRecurdByID?Patientid={patientId}");

            int recordId;

            if (checkRecordResponse.IsSuccessStatusCode)
            {
                var recordIdStr = await checkRecordResponse.Content.ReadAsStringAsync();

                if (int.TryParse(recordIdStr, out var existingRecordId) && existingRecordId != 0)
                {
                    // وجدنا سجل موجود، نستخدمه
                    recordId = existingRecordId;
                }
                else
                {
                    // ما في سجل موجود، نضيف سجل جديد
                    var dTO = new DTORecordPatients
                    {
                        PatientId = patientId,
                        TechnicianiD = technicianId,
                        RequestDate = DateTime.Now
                    };

                    var JsonContent = new StringContent(JsonConvert.SerializeObject(dTO), Encoding.UTF8, "application/json");
                    var addRecordResponse = await _httpClient.PostAsync($"https://localhost:7116/api/RecordPatients/AddRecurd", JsonContent);

                    if (!addRecordResponse.IsSuccessStatusCode)
                    {
                        var errorResponse = await addRecordResponse.Content.ReadAsStringAsync();
                        TempData["Error"] = $"حدث خطأ أثناء اضافة السجل: {errorResponse}";
                        return RedirectToAction("Index");
                    }

                    var addedRecordIdStr = await addRecordResponse.Content.ReadAsStringAsync();
                    recordId = int.Parse(addedRecordIdStr);
                }
            }
            else
            {
                // لم نتمكن من التحقق من السجل، يمكن التعامل مع الخطأ هنا
                TempData["Error"] = "تعذر التحقق من وجود السجل.";
                return RedirectToAction("Index");
            }

            // حفظ رقم السجل في الجلسة
            HttpContext.Session.SetInt32("SeleceRecurdiD", recordId);

            // إضافة بيانات الجدول الوسيط
            var requestTestJson = HttpContext.Session.GetString("SeleceRequestTestiDs");
            var requestTestIds = JsonConvert.DeserializeObject<List<int>>(requestTestJson);

            var dTOs = new DTOAddRecordRequestTest
            {
                RecordId = recordId,
                RequestTestId = requestTestIds
            };

            var JsonContents = new StringContent(JsonConvert.SerializeObject(dTOs), Encoding.UTF8, "application/json");
            var responsess = await _httpClient.PostAsync($"https://localhost:7116/api/RecordRequestTest", JsonContents);

            if (!responsess.IsSuccessStatusCode)
            {
                var errorResponse = await responsess.Content.ReadAsStringAsync();
                TempData["Error"] = $"حدث خطأ أثناء إضافة البيانات الوسيطة: {errorResponse}";
                return RedirectToAction("Index");
            }

            // جلب بيانات التحاليل
            var query = string.Join("&", selectedTestIds.Select(i => $"id={i}"));
            var url = $"https://localhost:7116/api/Test/GetTestByListId?{query}";

            var respons = await _httpClient.GetAsync(url);

            if (!respons.IsSuccessStatusCode)
                return StatusCode((int)respons.StatusCode, "حدث خطأ عند جلب البيانات");

            var data = await respons.Content.ReadFromJsonAsync<List<DTOTest>>(new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (data != null && data.Any())
            {
                var jsons = JsonConvert.SerializeObject(data);
                HttpContext.Session.SetString("SelectedTests", jsons);
            }
            //جلب بيانات الاعدادات بتاع النظام
            var respnse = await _httpClient.GetAsync($"https://localhost:7116/api/SettingSystem/GetAllSettingData");
            var jsonss = await respnse.Content.ReadAsStringAsync();
            HttpContext.Session.SetString("SelectedData", jsonss);

            return RedirectToAction(nameof(PrintSelectedTests));
        }





        public  IActionResult PrintSelectedTests()
        {

            //جلب بيانات الاعدادات من السيشت

            var  setting=  HttpContext.Session.GetString("SelectedData");
            var result = JsonConvert.DeserializeObject<DTOSettingSystem>(setting);
            // جلب بيانات التحاليل من TempData
            var testsJson = HttpContext.Session.GetString("SelectedTests");
            if (testsJson == null)
            {
                TempData["error"] = "لا يوجد بيانات ";
            }

            var tests = JsonConvert.DeserializeObject<List<DTOTest>>(testsJson);

            if (tests == null)
            {
                TempData["Error"] = "لا توجد بيانات للطباعة.";
                return RedirectToAction("Index", "Patient");
            }

            // جلب بيانات المريض من seetion
            int? patientId = HttpContext.Session.GetInt32("SelectedPatientID");
            string patientName = HttpContext.Session.GetString("PatientName") ?? "-";

            if (patientId == null)
            {
                TempData["Error"] = "رقم المريض غير موجود في الجلسة.";
                return RedirectToAction("Index", "Patient");
            }

            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);
            /////
            var headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 2, 1, 2 }))
               .UseAllAvailableWidth();
            headerTable.SetBorder(Border.NO_BORDER);

            // العمود الأول: الاسم + العنوان
            var leftCell = new Cell()
                .Add(new Paragraph(result?.Name ?? "Dakass Specialized Medical Laboratories")
                    .SetBold().SetFontSize(12))

                .Add(new Paragraph(result?.Addrees ?? "Ibb Street, Al-Thawra, next to the Arab Bank")
                    .SetFontSize(10))
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT);
            headerTable.AddCell(leftCell);

            // العمود الثاني: الصورة في الوسط
            var image = new Image(ImageDataFactory.Create($"wwwroot/{result?.Image}" ?? "not Found Image Now"))
                .ScaleToFit(100, 100)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            var imageCell = new Cell()
                .Add(image)
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER);
            headerTable.AddCell(imageCell);

            // العمود الثالث: البريد + الهاتف
            var rightCell = new Cell()
                .Add(new Paragraph(result?.Email ?? "alhatmysahr24@gmail.com").SetFontSize(10))
                .Add(new Paragraph(result?.PhoneNumber ?? "0777484844").SetFontSize(10))
                .Add(new Paragraph(result?.Descraption ?? "؟؟؟؟؟؟؟؟؟؟؟؟؟").SetFontSize(10))
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.RIGHT);
            headerTable.AddCell(rightCell);

            // إضافة الرأس للمستند
            document.Add(headerTable);

            // خط فاصل أنيق تحت الرأس
            document.Add(new LineSeparator(new SolidLine()));

            // عنوان التقرير
            document.Add(new Paragraph("Laboratory Tests Request")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(16)
                .SetMarginBottom(15));

            // معلومات المريض
            document.Add(new Paragraph($"Patient ID: {patientId}").SetFontSize(12));
            document.Add(new Paragraph($"Patient Name: {patientName}").SetFontSize(12));
            document.Add(new Paragraph($"Date: {DateTime.Now:yyyy-MM-dd}").SetFontSize(12)
                .SetMarginBottom(15));

            document.Add(new LineSeparator(new SolidLine()).SetMarginBottom(10));
            // تعريف العنوان
            var header = new Paragraph()
                .AddTabStops(new TabStop(300, TabAlignment.LEFT)) // المحاذاة للعمود الثاني
                .Add("Test Name")    // عنوان العمود الأول
                .Add(new Tab())
                .Add("Price")        // عنوان العمود الثاني
                .SetBold()
                .SetFontSize(12)
                .SetMarginBottom(5);
            document.Add(header);

            // إضافة بيانات التحاليل
            foreach (var test in tests)
            {
                var paragraph = new Paragraph()
                    .AddTabStops(new TabStop(300, TabAlignment.LEFT))
                    .Add(test.TestNameEn ?? "-")                          // اسم التحليل
                    .Add(new Tab())
                    .Add(test.Testprice.ToString("0.##") + " ر.ي")        // السعر
                    .SetFontSize(12)
                    .SetMarginBottom(3);
                document.Add(paragraph);
            }


            var total = tests.Sum(t => t.Testprice);
            document.Add(new Paragraph(new string('-', 40)));
            document.Add(new Paragraph($"Total Price: {total:0.##} ر.ي")
           .SetBold()
           .SetFontSize(12)
           .SetTextAlignment(TextAlignment.LEFT)
           .SetMarginTop(10));



            document.Add(new Paragraph($"Lis System - {DateTime.Now:yyyy-MM-dd}")
           .SetTextAlignment(TextAlignment.CENTER)
           .SetFontSize(9)
           .SetMarginTop(20));

            document.Close();

            var pdfBytes = stream.ToArray();
            return File(pdfBytes, "application/pdf");
        }


        //داله لاضافة البيانات الى الجدو الوسيط بين الجدول الوسيط والسجلات الطبيه
    }
}
