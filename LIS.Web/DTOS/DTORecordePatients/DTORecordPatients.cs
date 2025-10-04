using System.ComponentModel.DataAnnotations;

namespace مشروع_ادار_المختبرات.DTOS
{
    public class DTORecordPatients
    {
        public int PatientId { get; set; }


        public DateTime RequestDate { get; set; }

        public int TechnicianiD { get; set; }

    }
}
