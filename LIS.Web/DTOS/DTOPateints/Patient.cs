using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace مشروع_ادار_المختبرات.Models
{
    public class Patient
    {
        [Key]
        public int PatientID { get; set; }
        public string? FullName { get; set; }
        public DateTime BirthDate { get; set; }
        public bool Gender { get; set; }
        public string? phoneNumber { get; set; }
        public string? Password { get; set; }

        [Required]
        public string? Address { get; set; }
        public int? SupervisorID { get; set; } = 1;
        //public ICollection<Sample>? samples { get; set; }

    }
}
