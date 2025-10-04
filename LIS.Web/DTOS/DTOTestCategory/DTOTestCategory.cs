using System.ComponentModel.DataAnnotations;

namespace مشروع_ادار_المختبرات.DTOS
{
    public class DTOTestCategory
    {
        [Key]
        public int CategoryId { get; set; }
        [Required]
        [StringLength(100)]
        public string CategoryNameEn { get; set; }
        [Required]
        [StringLength(100)]
        public string CategoryNameAr { get; set; }
    }
}
