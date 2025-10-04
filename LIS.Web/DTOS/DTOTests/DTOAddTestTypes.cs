using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace مشروع_ادار_المختبرات.DTOS
{
    public class DTOAddTestTypes
    {
        [Key]
        public int TestId { get; set; }

        [Required, StringLength(200)]
        public string TestNameEn { get; set; }

        [Required, StringLength(200)]
        public string TestNameAr { get; set; }

        [Required, StringLength(200)]
        public string SampleType { get; set; }

        public string NormalRange { get; set; }

        public int Testprice { get; set; }

        [ForeignKey("TestCategory")]
        public int CategoryId { get; set; }

    }
}
