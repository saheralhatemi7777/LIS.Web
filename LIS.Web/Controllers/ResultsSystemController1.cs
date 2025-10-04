using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using مشروع_ادار_المختبرات.DTOS;
using System.Net.Http;
using iText.Kernel.Colors;
using iText.Kernel.Pdf.Canvas.Draw;
using iText.Layout.Borders;
using iText.IO.Image;
using System.Text.Json;

namespace مشروع_ادار_المختبرات.Controllers
{
    public class ResultsSystemController : Controller
    {
        
        //الكنترول الحاص بتقارير الادارة العامه للسستم
        public ResultsSystemController(IHttpClientFactory httpClientFactory)
        {
            _httpClient = httpClientFactory.CreateClient();
        }
        private readonly HttpClient _httpClient;
        private readonly string _apiBaseUrl = "https://localhost:7116/api";

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> PrintAllPatients()
        {
            var respnse = await _httpClient.GetAsync($"https://localhost:7116/api/SettingSystem/GetAllSettingData");
            var jsons = await respnse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<DTOSettingSystem>(jsons);

            // جلب بيانات المرضى
            var response = await _httpClient.GetAsync($"{_apiBaseUrl}/Patient");
            if (!response.IsSuccessStatusCode)
                return BadRequest("فشل تحميل البيانات من API.");

            var json = await response.Content.ReadAsStringAsync();
            var patients = JsonConvert.DeserializeObject<List<DTOPatients>>(json);

            var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // إعداد الخط
            var font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            document.SetFont(font);

            // ---------------------------
            // رأس الصفحة (يسار ↔ يمين)
            // ---------------------------
            // رأس الصفحة بمستوى واحد
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
            var image = new Image(ImageDataFactory.Create($"wwwroot/{result?.Image}"))
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


            // ---------------------------
            // عنوان التقرير
            // ---------------------------
            document.Add(new Paragraph("Patients Report")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(18)
                .SetMarginBottom(10));

            // ---------------------------
            // جدول المرضى
            // ---------------------------
            var table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 5, 2, 1, 2, 2 }))
                           .UseAllAvailableWidth();
            DeviceRgb headerColor = new DeviceRgb(200, 216, 230);

            table.AddHeaderCell(new Cell().Add(new Paragraph("ID").SetBold()).SetBackgroundColor(headerColor));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Name").SetBold()).SetBackgroundColor(headerColor));
            table.AddHeaderCell(new Cell().Add(new Paragraph("DateOfBirth").SetBold()).SetBackgroundColor(headerColor));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Gender").SetBold()).SetBackgroundColor(headerColor));
            table.AddHeaderCell(new Cell().Add(new Paragraph("PhoneNumber").SetBold()).SetBackgroundColor(headerColor));
            table.AddHeaderCell(new Cell().Add(new Paragraph("Address").SetBold()).SetBackgroundColor(headerColor));

            foreach (var p in patients)
            {
                table.AddCell(new Paragraph(p.PatientID.ToString()));
                table.AddCell(new Paragraph(p.FullName));
                table.AddCell(new Paragraph(p.BirthDate.ToString("yyyy-MM-dd")));
                table.AddCell(new Paragraph(p.Gender ? "Male" : "Female"));
                table.AddCell(new Paragraph(p.phoneNumber ?? "-"));
                table.AddCell(new Paragraph(p.Address ?? "-"));
            }

            document.Add(table);

            // ---------------------------
            // التذييل
            // ---------------------------
            document.Add(new Paragraph($"Lis System - {DateTime.Now:yyyy-MM-dd}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(9)
                .SetMarginTop(20));

            document.Close();

            var pdfBytes = stream.ToArray();
            return File(pdfBytes, "application/pdf");
        }

        [HttpGet]
        public async Task<IActionResult> PrintTestResults()
        {
            //جلب بيانات اعدادت النظام
            var respnse = await _httpClient.GetAsync($"https://localhost:7116/api/SettingSystem/GetAllSettingData");
            var jsons = await respnse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<DTOSettingSystem>(jsons);


            // استدعاء الـ API
            HttpResponseMessage response = await _httpClient.GetAsync($"https://localhost:7116/api/TestResult/GetAllTestResult" );

            if (!response.IsSuccessStatusCode)
            {
                return Content($"خطأ عند جلب البيانات من API: {response.StatusCode}");
            }

            var json = await response.Content.ReadAsStringAsync();
            var patientsResults = Newtonsoft.Json.JsonConvert.DeserializeObject<List<RequestResultDto>>(json);

            if (patientsResults == null || !patientsResults.Any())
            {
                return Content("لا توجد بيانات للتقرير");
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
            var image = new Image(ImageDataFactory.Create($"wwwroot/{result?.Image}"))
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

            document.Add(new Paragraph("Laboratory Test Results Report")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetBold()
                .SetFontSize(16)
                .SetFontColor(ColorConstants.BLACK)
                .SetMarginBottom(20));


            foreach (var patient in patientsResults)
            {


                var patientInfoTable = new Table(2).UseAllAvailableWidth();


                patientInfoTable.SetBorder(Border.NO_BORDER).SetBackgroundColor(new DeviceRgb(230, 245, 250)); 



                patientInfoTable.AddCell(new Cell().Add(new Paragraph("Patient Name:").SetBold()).SetBorder(Border.NO_BORDER));
                patientInfoTable.AddCell(new Cell().Add(new Paragraph(patient.PatientName ?? "-")).SetBorder(Border.NO_BORDER));

                patientInfoTable.AddCell(new Cell().Add(new Paragraph("Status Request:").SetBold()).SetBorder(Border.NO_BORDER));
                patientInfoTable.AddCell(new Cell().Add(new Paragraph("The test has been issued")).SetBorder(Border.NO_BORDER));

                patientInfoTable.AddCell(new Cell().Add(new Paragraph("Supervisor:").SetBold()).SetBorder(Border.NO_BORDER));
                patientInfoTable.AddCell(new Cell().Add(new Paragraph(patient.SupervisorName ?? "-")).SetBorder(Border.NO_BORDER));

                patientInfoTable.AddCell(new Cell().Add(new Paragraph("Lab Technician:").SetBold()).SetBorder(Border.NO_BORDER));
                patientInfoTable.AddCell(new Cell().Add(new Paragraph(patient.LabTechnicianName ?? "-")).SetBorder(Border.NO_BORDER));

                patientInfoTable.AddCell(new Cell().Add(new Paragraph("Create At:").SetBold()).SetBorder(Border.NO_BORDER));
                patientInfoTable.AddCell(new Cell().Add(new Paragraph(patient.createAt.ToString("yyyy-MM-dd"))).SetBorder(Border.NO_BORDER));

                document.Add(patientInfoTable);
                document.Add(new Paragraph("\n")); // مسافة قبل جدول التحاليل

                if (patient.Tests == null || !patient.Tests.Any())
                {
                    document.Add(new Paragraph("No tests found for this patient.\n")
                        .SetFontColor(ColorConstants.RED));
                    continue;
                }

                // جدول التحاليل (يبقى كما هو)
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 3, 2, 4, 2 }))
                    .UseAllAvailableWidth();


                // الهيدر مع الخلفية والخطوط البيضاء
                table.AddHeaderCell(new Cell().Add(new Paragraph("Test Name").SetBold().SetFontColor(ColorConstants.WHITE))
                    .SetBackgroundColor(ColorConstants.DARK_GRAY));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Result").SetBold().SetFontColor(ColorConstants.WHITE))
                    .SetBackgroundColor(ColorConstants.DARK_GRAY));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Reference Range").SetBold().SetFontColor(ColorConstants.WHITE))
                    .SetBackgroundColor(ColorConstants.DARK_GRAY));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Creation Date").SetBold().SetFontColor(ColorConstants.WHITE))
                    .SetBackgroundColor(ColorConstants.DARK_GRAY));

                // صفوف البيانات بدون حدود
                foreach (var test in patient.Tests)
                {
                    table.AddCell(new Cell().Add(new Paragraph(test.TestName ?? "-").SetFontColor(ColorConstants.BLACK))
                        .SetBorder(Border.NO_BORDER));
                    table.AddCell(new Cell().Add(new Paragraph(test.ResultValue ?? "-").SetFontColor(ColorConstants.BLUE))
                        .SetBorder(Border.NO_BORDER));
                    table.AddCell(new Cell().Add(new Paragraph(test.ReferenceRange ?? "-").SetFontColor(ColorConstants.BLACK))
                        .SetBorder(Border.NO_BORDER));
                    table.AddCell(new Cell().Add(new Paragraph(test.CreatedAt.ToString("yyyy-MM-dd")).SetFontColor(ColorConstants.BLACK))
                        .SetBorder(Border.NO_BORDER));
                }


             
                document.Add(table);
                document.Add(new Paragraph("\n_______________________________________________________________________________\n")
                    .SetFontColor(ColorConstants.LIGHT_GRAY));
            }

            // توقيع المشرف
            document.Add(new Paragraph($"Supervisor’s Signature: ____________________")
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(10)
                .SetFontColor(ColorConstants.BLACK)
                .SetMarginTop(20));

            // تذييل التقرير
            document.Add(new Paragraph($"Lis System - {DateTime.Now:yyyy-MM-dd}")
                .SetTextAlignment(TextAlignment.CENTER)
                .SetFontSize(9)
                .SetFontColor(ColorConstants.DARK_GRAY)
                .SetMarginTop(20));

            document.Close();

            var pdfBytes = stream.ToArray();
            return File(pdfBytes, "application/pdf");
        }

       
       
        public async Task<IActionResult> GetAllSetteng()
        {
            using var httpClient = new HttpClient();
            var apiUrl = "https://localhost:7116/api/DoctorActivityDto/MostActiveDoctors";

            // استدعاء API وتحويل JSON إلى List<DoctorActivityDto>
            var doctors = await httpClient.GetFromJsonAsync<List<DoctorActivityDto>>(apiUrl)
                          ?? new List<DoctorActivityDto>();

            // تمرير البيانات للـ View بصيغة JSON لاستخدامها في JavaScript
            ViewBag.DoctorsJson = System.Text.Json.JsonSerializer.Serialize(doctors, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            });
            return View();
        }


        public async Task<IActionResult> PrintAllTest()
        {
            var respnse = await _httpClient.GetAsync($"https://localhost:7116/api/SettingSystem/GetAllSettingData");
            var jsons = await respnse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<DTOSettingSystem>(jsons);
            HttpResponseMessage response = await _httpClient.GetAsync(
                           $"https://localhost:7116/api/Test/test"
                       );

            var json = await response.Content.ReadAsStringAsync();


            var tests = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DTOTestsType>>(json);

            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
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
                var image = new Image(ImageDataFactory.Create($"wwwroot/{result?.Image}"))
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
                // 📌 إضافة عنوان التقرير
                Paragraph header = new Paragraph("Report of All Tests in the System")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(18)
                    .SetBold();
                document.Add(header);

                document.Add(new Paragraph("\n")); // سطر فارغ بعد العنوان

                // 📌 إنشاء الجدول مع 5 أعمدة
                var table = new Table(UnitValue.CreatePercentArray(new float[] { 1, 2, 2, 5, 2 }))
                    .UseAllAvailableWidth();
                // تلوين رأس الجدول باللون الأزرق الفاتح
                DeviceRgb headerColor = new DeviceRgb(200, 216, 230);

                table.AddHeaderCell(new Cell().Add(new Paragraph("ID")).SetBackgroundColor(headerColor));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Name (EN)")).SetBackgroundColor(headerColor));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Sample Type")).SetBackgroundColor(headerColor));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Normal Range")).SetBackgroundColor(headerColor));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Price")).SetBackgroundColor(headerColor));

                // إضافة بيانات التحاليل
                foreach (var test in tests)
                {
                    table.AddCell(new Paragraph(test.TestId.ToString()));
                    table.AddCell(new Paragraph(test.TestNameEn ?? ""));
                    table.AddCell(new Paragraph(test.SampleType ?? ""));
                    table.AddCell(new Paragraph(test.NormalRange ?? ""));
                    table.AddCell(new Paragraph(test.Testprice.ToString()));
                }

                document.Add(table);

                // 📌 إضافة تذييل (Footer) باسم النظام وتاريخ الطباعة
                document.Add(new Paragraph("\n\n"));
                LineSeparator line = new LineSeparator(new SolidLine());
                document.Add(line);

                string footerText = $"System: Medical Lab Management System(Lis)   |   Printed on: {DateTime.Now:yyyy-MM-dd HH:mm}";
                Paragraph footer = new Paragraph(footerText)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetItalic();

                document.Add(footer);

                document.Close();

                return File(ms.ToArray(), "application/pdf");
            }
        }

        public async Task<IActionResult> PrintAllCategorTest()
        {
            var respnse = await _httpClient.GetAsync($"https://localhost:7116/api/SettingSystem/GetAllSettingData");
            var jsons = await respnse.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<DTOSettingSystem>(jsons);
            HttpResponseMessage response = await _httpClient.GetAsync(
                           $"https://localhost:7116/api/TestCategory"
                       );

            var json = await response.Content.ReadAsStringAsync();
         
            var tests = Newtonsoft.Json.JsonConvert.DeserializeObject<List<DTOTestCategory>>(json);

            using (var ms = new MemoryStream())
            {
                var writer = new PdfWriter(ms);
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
                var image = new Image(ImageDataFactory.Create($"wwwroot/{result?.Image}"))
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
                // 📌 إضافة عنوان التقرير
                Paragraph header = new Paragraph("Report of All CategoryTests in the System")
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(18)
                    .SetBold();
                document.Add(header);

                document.Add(new Paragraph("\n")); // سطر فارغ بعد العنوان

                // 📌 إنشاء الجدول مع 5 أعمدة
                Table table = new Table(2, true);

                // تلوين رأس الجدول باللون الأزرق الفاتح
                DeviceRgb headerColor = new DeviceRgb(200, 216, 230);

                table.AddHeaderCell(new Cell().Add(new Paragraph("ID")).SetBackgroundColor(headerColor));
                table.AddHeaderCell(new Cell().Add(new Paragraph("Name ")).SetBackgroundColor(headerColor));
               
                // إضافة بيانات التحاليل
                foreach (var test in tests)
                {
                    table.AddCell(new Paragraph(test.CategoryId.ToString()));
                    table.AddCell(new Paragraph(test.CategoryNameEn ?? ""));
                    
                }

                document.Add(table);

                // 📌 إضافة تذييل (Footer) باسم النظام وتاريخ الطباعة
                document.Add(new Paragraph("\n\n"));
                LineSeparator line = new LineSeparator(new SolidLine());
                document.Add(line);

                string footerText = $"System: Medical Lab Management System(Lis)   |   Printed on: {DateTime.Now:yyyy-MM-dd HH:mm}";
                Paragraph footer = new Paragraph(footerText)
                    .SetTextAlignment(iText.Layout.Properties.TextAlignment.CENTER)
                    .SetFontSize(10)
                    .SetItalic();

                document.Add(footer);

                document.Close();

                return File(ms.ToArray(), "application/pdf");

            }
        }

    }
}
