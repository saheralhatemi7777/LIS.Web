namespace مشروع_ادار_المختبرات.DTOS
{
    public class RequestDto
    {
        public int RequestID { get; set; }
        public string PatientName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<TestDto> Tests { get; set; }
    }
}
