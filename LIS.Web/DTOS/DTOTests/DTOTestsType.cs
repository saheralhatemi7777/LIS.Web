using System.ComponentModel.DataAnnotations;

namespace مشروع_ادار_المختبرات.DTOS
{
    public class DTOTestsType
    {

        [Key]
        public int TestId { get; set; }

        public string TestNameEn { get; set; }

        public string TestNameAr { get; set; }

        public string SampleType { get; set; }

        public string NormalRange { get; set; }

        public string CategoryNameEn { get; set; }

        public string CategoryNameAr { get; set; }
        public int Testprice { get; set; }
        public int? categoryid { get; set; }

    }
}
