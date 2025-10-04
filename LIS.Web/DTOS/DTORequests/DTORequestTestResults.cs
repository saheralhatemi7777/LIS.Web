using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace مشروع_ادار_المختبرات.DTOS
{
    public class PatientDto
    {
        public int PatientID { get; set; }
        public string PatientName { get; set; }
        public List<RequestDTO> Requests { get; set; }
    }

    public class RequestDTO
    {
        public int RequestID { get; set; }
        public int RequestTestID { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public string DoctorName { get; set; }
        public List<TestDTO> Tests { get; set; }
    }

    public class TestDTO
    {
        public string Name { get; set; }
        public string Samples { get; set; }
        public decimal Price { get; set; }
    }

    
  
}