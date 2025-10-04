using System.ComponentModel.DataAnnotations;

namespace مشروع_ادار_المختبرات.DTOS
{
    public class DTORequest
    {

        [Key]
        public int RecuestTestID { get; set; }

        [Required]
        public int PatientID { get; set; }

        public string Status { get; set; }

        public string? Notes { get; set; }

        public DateTime CreatedAt { get; set; }

        public string usernName { get;  set; }

        public string? PatientName { get;  set; }

        public int UserID { get; internal set; }
    }

}
