namespace مشروع_ادار_المختبرات.DTOS
{
    public class PatientRecordDto
    {
        public int PatientId { get; set; }
        public string FullName { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string Username { get; set; }
        public int RecordId { get; set; }
        public DateTime CreateAt { get; set; }   
    }

}
