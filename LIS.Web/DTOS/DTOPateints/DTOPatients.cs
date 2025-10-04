using System;

namespace مشروع_ادار_المختبرات.DTOS
{
    public class DTOPatients
    {

        public int PatientID { get; set; }
        public string? FullName { get; set; }
        public DateTime BirthDate { get; set; }
        public bool Gender { get; set; }
        public string? phoneNumber { get; set; }
        public string? Password { get; set; }
        public string? Address { get; set; }
        public int? SupervisorID { get; set; }
        public object? UserName { get; set; }


    }
}
