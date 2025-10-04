using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text;
using مشروع_ادار_المختبرات.DTOS;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using System.Net.Http;
using iText.Kernel.Colors;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout.Borders;
using iText.IO.Image;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class RecordPatientsController : Controller
    {
        

        public RecordPatientsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }

        private readonly HttpClient _httpClient;
        public async Task<IActionResult> Index(string name)
        {
            if (!string.IsNullOrEmpty(name))
            {
                var patient = await _httpClient.GetFromJsonAsync<List<PatientRecordDto>>(
               $"https://localhost:7116/api/RecordRequestTest/GetAllRecordRequestTestByName?name={name}");
                return View(patient);
            }
            //API استدعاء   واستقبال البيانات مباشرة كمصفوفة من المرضى
            var patients = await _httpClient.GetFromJsonAsync<List<PatientRecordDto>>(
            "https://localhost:7116/api/RecordRequestTest/GetAllRecordRequestTest");

            return View(patients);
        }


        public async Task<IActionResult> GetToRecordPatients(int id)
        {
            var respons = await _httpClient.GetFromJsonAsync<PatientData>($"https://localhost:7116/api/RecordRequestTest/GetAllRecordRequestByiDTest?id={id}");
            return View(respons);   
        }

        [HttpGet]
        public async Task<IActionResult> PrintTestResults(int id)
        {
            var respnse = await _httpClient.GetAsync($"https://localhost:7116/api/SettingSystem/GetAllSettingData");
            var jsons = await respnse.Content.ReadAsStringAsync();
            var results = JsonConvert.DeserializeObject<DTOSettingSystem>(jsons);
            var response = await _httpClient.GetAsync($"https://localhost:7116/api/RecordRequestTest/GetAllRecordRequestByiDTest?id={id}");

            if (!response.IsSuccessStatusCode)
            {
                return Content($"خطأ عند جلب البيانات من API: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var patient = JsonConvert.DeserializeObject<PatientData>(json);

            if (patient == null)
                return Content("لا توجد بيانات للتقرير");

            var record = patient.Records.FirstOrDefault(r => r.Id == id);

            if (record == null)
                return Content($"لم يتم العثور على السجل رقم {id} لهذا المريض.");

            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // ألوان
            var headerBgColor = new DeviceRgb(52, 152, 219);      // أزرق سماوي
            var headerFontColor = ColorConstants.WHITE;
            var rowAltColor = new DeviceRgb(245, 245, 245);       // رمادي فاتح
            /////
            var headerTable = new Table(UnitValue.CreatePercentArray(new float[] { 2, 1, 2 }))
               .UseAllAvailableWidth();
            headerTable.SetBorder(Border.NO_BORDER);

            // العمود الأول: الاسم + العنوان
            var leftCell = new Cell()
                .Add(new Paragraph(results?.Name ?? "Dakass Specialized Medical Laboratories")
                    .SetBold().SetFontSize(12))
                .Add(new Paragraph(results?.Addrees ?? "Ibb Street, Al-Thawra, next to the Arab Bank")
                    .SetFontSize(10))
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.LEFT);
            headerTable.AddCell(leftCell);

            // العمود الثاني: الصورة في الوسط
            var image = new Image(ImageDataFactory.Create($"wwwroot/{results?.Image}"))
                .ScaleToFit(100, 100)
                .SetHorizontalAlignment(HorizontalAlignment.CENTER);

            var imageCell = new Cell()
                .Add(image)
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.CENTER);
            headerTable.AddCell(imageCell);

            // العمود الثالث: البريد + الهاتف
            var rightCell = new Cell()
                .Add(new Paragraph(results?.Email ?? "alhatmysahr24@gmail.com").SetFontSize(10))
                .Add(new Paragraph(results?.PhoneNumber ?? "0777484844").SetFontSize(10))
                .Add(new Paragraph(results?.Descraption ?? "؟؟؟؟؟؟؟؟؟؟؟؟؟").SetFontSize(10))
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.RIGHT);
            headerTable.AddCell(rightCell);

            // إضافة الرأس للمستند
            document.Add(headerTable);

            // خط فاصل أنيق تحت الرأس
            document.Add(new LineSeparator(new SolidLine()));
            // عنوان التقرير
            document.Add(new Paragraph("💉 Laboratory Test Results Report")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(16)
                .SetFontColor(ColorConstants.DARK_GRAY)
                .SetMarginBottom(20));

            // بيانات المريض والسجل
            var infoTable = new Table(2).UseAllAvailableWidth();

            infoTable.AddCell(new Cell().Add(new Paragraph("👤 Patient Name:").SetBold()).SetBorder(Border.NO_BORDER));
            infoTable.AddCell(new Cell().Add(new Paragraph(patient.FullName)).SetBorder(Border.NO_BORDER));

            infoTable.AddCell(new Cell().Add(new Paragraph("📞 Phone:").SetBold()).SetBorder(Border.NO_BORDER));
            infoTable.AddCell(new Cell().Add(new Paragraph(patient.Phone)).SetBorder(Border.NO_BORDER));

            infoTable.AddCell(new Cell().Add(new Paragraph("🏠 Address:").SetBold()).SetBorder(Border.NO_BORDER));
            infoTable.AddCell(new Cell().Add(new Paragraph(patient.Address ?? "-")).SetBorder(Border.NO_BORDER));

            infoTable.AddCell(new Cell().Add(new Paragraph("🧑‍⚕️ Username:").SetBold()).SetBorder(Border.NO_BORDER));
            infoTable.AddCell(new Cell().Add(new Paragraph(patient.Username ?? "-")).SetBorder(Border.NO_BORDER));

            infoTable.AddCell(new Cell().Add(new Paragraph("📂 Record #:").SetBold()).SetBorder(Border.NO_BORDER));
            infoTable.AddCell(new Cell().Add(new Paragraph(record.Id.ToString())).SetBorder(Border.NO_BORDER));

            infoTable.AddCell(new Cell().Add(new Paragraph("🗓️ Record Date:").SetBold()).SetBorder(Border.NO_BORDER));
            infoTable.AddCell(new Cell().Add(new Paragraph(record.Date.ToString("yyyy-MM-dd"))).SetBorder(Border.NO_BORDER));

            document.Add(infoTable.SetMarginBottom(20));


            foreach (var request in record.Requests)
            {
                // فاصل بين الطلبات
                document.Add(new LineSeparator(new SolidLine()).SetMarginBottom(10));

                // عنوان الطلب
                document.Add(new Paragraph($"🧪 Request #{request.Id}")
                    .SetFontSize(12)
                    .SetBold()
                    .SetFontColor(ColorConstants.BLUE));

                document.Add(new Paragraph($"📅 Created At: {request.Date}").SetFontSize(10));
                document.Add(new Paragraph($"📌 Status:{request.Status} The test has been successfully issued")
                    .SetFontSize(10)
                    .SetItalic()
                    .SetMarginBottom(10));

                if (request.Tests == null || !request.Tests.Any())
                {
                    document.Add(new Paragraph("⚠️ No tests found for this request.\n"));
                    continue;
                }

                // جدول التحاليل
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 2, 3, 2 }))
                    .UseAllAvailableWidth();

                // ترويسة الجدول
                table.AddHeaderCell(new Cell().Add(new Paragraph("🧪 Test Name").SetBold().SetFontColor(headerFontColor))
                    .SetBackgroundColor(headerBgColor));
                table.AddHeaderCell(new Cell().Add(new Paragraph("📈 Result").SetBold().SetFontColor(headerFontColor))
                    .SetBackgroundColor(headerBgColor));
                table.AddHeaderCell(new Cell().Add(new Paragraph("📏 Normal Range").SetBold().SetFontColor(headerFontColor))
                    .SetBackgroundColor(headerBgColor));
                table.AddHeaderCell(new Cell().Add(new Paragraph("📅 Created At").SetBold().SetFontColor(headerFontColor))
                    .SetBackgroundColor(headerBgColor));

                int rowIndex = 0;

                foreach (var test in request.Tests)
                {
                    var bgColor = rowIndex % 2 == 0 ? ColorConstants.WHITE : rowAltColor;

                    if (test.Results != null && test.Results.Any())
                    {
                        foreach (var result in test.Results)
                        {
                            table.AddCell(new Cell().Add(new Paragraph($"{test.EnglishName} ({test.ArabicName})")).SetBackgroundColor(bgColor));
                            table.AddCell(new Cell().Add(new Paragraph(result.Value ?? "-")).SetBackgroundColor(bgColor));
                            table.AddCell(new Cell().Add(new Paragraph(test.NormalRange ?? "-")).SetBackgroundColor(bgColor));
                            table.AddCell(new Cell().Add(new Paragraph(result.CreatedAt.ToString())).SetBackgroundColor(bgColor));
                        }
                    }
                    else
                    {
                        table.AddCell(new Cell().Add(new Paragraph($"{test.EnglishName} ({test.ArabicName})")).SetBackgroundColor(bgColor));
                        table.AddCell(new Cell().Add(new Paragraph(test.ResultValue ?? "-")).SetBackgroundColor(bgColor));
                        table.AddCell(new Cell().Add(new Paragraph(test.NormalRange ?? "-")).SetBackgroundColor(bgColor));
                        table.AddCell(new Cell().Add(new Paragraph("-")).SetBackgroundColor(bgColor));
                    }

                    rowIndex++;
                }

                document.Add(table);
                document.Add(new Paragraph("\n"));
            }

            document.Add(new Paragraph($"Doctor's Signature:______________________")
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(10)
               .SetFontColor(ColorConstants.GRAY)
               .SetBackgroundColor(ColorConstants.WHITE)
               .SetMarginTop(30));
            // التذييل
            document.Add(new Paragraph($"Generated by LIS System - {DateTime.Now:yyyy-MM-dd HH:mm}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(9)
                .SetFontColor(ColorConstants.GRAY)
                .SetBold()
                .SetMarginTop(30));

            document.Close();

            var pdfBytes = stream.ToArray();
            return File(pdfBytes, "application/pdf");
        }

    }
}
