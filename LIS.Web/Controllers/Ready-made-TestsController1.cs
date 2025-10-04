using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using مشروع_ادار_المختبرات.DTOS;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using System.Net.Http.Json;
using iText.IO.Font;
using iText.Layout.Borders;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.IO.Image;
using iText.Kernel.Colors;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class Ready_made_TestsController : Controller
    {
        public Ready_made_TestsController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }
        private readonly HttpClient _httpClient;

        public async Task<IActionResult> Index(string name)
        {
            if(!string.IsNullOrEmpty(name))
            {
                var patient = await _httpClient.GetFromJsonAsync<List<PatientDto>>(
               $"https://localhost:7116/api/Ready_made_Tests/GetAllDataGroupedByPatientName?name={name}");
                return View(patient);
            }
                //API استدعاء   واستقبال البيانات مباشرة كمصفوفة من المرضى
                var patients = await _httpClient.GetFromJsonAsync<List<PatientDto>>(
                "https://localhost:7116/api/Ready_made_Tests/GetAllDataRequestTest");

            return View(patients);
        }



        [HttpGet]
        public async Task<IActionResult> PrintTestResults(int requestTestId)
        {
            var respnse = await _httpClient.GetAsync($"https://localhost:7116/api/SettingSystem/GetAllSettingData");
            var jsons = await respnse.Content.ReadAsStringAsync();

            var result = JsonConvert.DeserializeObject<DTOSettingSystem>(jsons);
            HttpResponseMessage response = await _httpClient.GetAsync(
                $"https://localhost:7116/api/TestResult/GetAllTestResultByRequestTestId?id={requestTestId}"
            );

            if (!response.IsSuccessStatusCode)
                return Content($" خطأ عند جلب البيانات من API: {response.StatusCode}");


            var requests = await response.Content.ReadFromJsonAsync<List<RequestResultDto>>();

            if (requests == null || !requests.Any())
                return Content(" لا توجد بيانات لهذا الطلب");


            var testResult = requests.First();

            if (testResult.Tests == null || !testResult.Tests.Any())
                return Content(" لا توجد تحاليل مرتبطة بهذا الطلب");

           
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
            var image = new Image(ImageDataFactory.Create($"wwwroot/{result?.Image}"??"not Found Image Now"))
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
                .SetBorder(Border.NO_BORDER)
                .SetTextAlignment(TextAlignment.RIGHT);
            headerTable.AddCell(rightCell);

            // إضافة الرأس للمستند
            document.Add(headerTable);

            // خط فاصل أنيق تحت الرأس
            document.Add(new LineSeparator(new SolidLine()));

            document.Add(new Paragraph("Laboratory Test Results Report")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(15)
                .SetMarginBottom(13));
            document.Add(new Paragraph($"Create At : {testResult.createAt}").SetBold().SetFontSize(12));
            document.Add(new Paragraph($"Patient Name : {testResult.PatientName}").SetBold().SetFontSize(12));
            document.Add(new Paragraph($"Status : SeeccsussFull").SetBold().SetFontSize(12));
            document.Add(new Paragraph($"Lab Technician: {testResult.LabTechnicianName ?? "-"}").SetBold().SetFontSize(12).SetMarginBottom(10));
            document.Add(new LineSeparator(new SolidLine()));

            // جدول التحاليل
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 2, 4, 2 }))
                .UseAllAvailableWidth();

            // رؤوس الجدول
            table.AddHeaderCell(new Cell().Add(new Paragraph("Test Name").SetFontColor(ColorConstants.BLUE).SetBold()).SetBorder(Border.NO_BORDER));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Result").SetFontColor(ColorConstants.BLUE).SetBold()).SetBorder(Border.NO_BORDER));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Reference Range").SetFontColor(ColorConstants.BLUE).SetBold()).SetBorder(Border.NO_BORDER));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Creation Date").SetFontColor(ColorConstants.BLUE).SetBold()).SetBorder(Border.NO_BORDER));

            // بيانات التحاليل
            foreach (var test in testResult.Tests)
            {

                table.AddCell(new Cell().Add(new Paragraph(test.TestName ?? "-")).SetBorder(Border.NO_BORDER));
                table.AddCell(new Cell().Add(new Paragraph(test.ResultValue ?? "-")).SetBorder(Border.NO_BORDER));
                table.AddCell(new Cell().Add(new Paragraph(test.ReferenceRange ?? "-")).SetBorder(Border.NO_BORDER));
                table.AddCell(new Cell().Add(new Paragraph(test.CreatedAt.ToString("yyyy-MM-dd"))).SetBorder(Border.NO_BORDER));

            }

            document.Add(table);

            // تذييل التقرير
            document.Add(new Paragraph($"Lis System - {DateTime.Now:yyyy-MM-dd}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(9)
                .SetMarginTop(20));

            document.Close();

            // إرجاع PDF
            var pdfBytes = stream.ToArray();
            return File(pdfBytes, "application/pdf");
        }
    }
}
