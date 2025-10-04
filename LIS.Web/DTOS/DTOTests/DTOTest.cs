using System.ComponentModel.DataAnnotations;

namespace APiUsers.DTOs.DTOSTests
{
    public class DTOTest
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


    }
}
