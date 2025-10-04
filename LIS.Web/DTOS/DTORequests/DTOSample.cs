using Newtonsoft.Json;
using System;
using System.ComponentModel.DataAnnotations;

namespace مشروع_ادار_المختبرات.DTOS
{
    public class DTORecuests
    {
        [Key]
        public int RecuestTestID { get; set; }
        [Required]
        public int PatientID { get; set; }
        [Required]
        public string? Status { get; set; }
        [Required]
        public string? Notes { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        public string? userName { get;  set; }
        public string? PatientName { get;  set; }
        public int? UserID { get;  set; }
    }
}
