using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace مشروع_ادار_المختبرات.DTOS
{
    public class DTOSettingSystem
    {
        public int SettingID { get; set; }
        public string? Name { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Addrees { get; set; }
        public string? Email { get; set; }
        public string? Descraption { get; set; }

        // هذا للمسار اللي ينخزن في  ملف المشروع
        public string? Image { get; set; }

        // هذا للحقل اللي يستقبل الملف من الفورم
        [NotMapped]
        public IFormFile? ImageFile { get; set; }
    }

}
