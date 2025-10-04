namespace مشروع_ادار_المختبرات.DTOS
{
    public class TestResultDto
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public string ReferenceRange { get; set; }
        public string ResultValue { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class RequestResultDto
    {
        public int RequestID { get; set; }
        public string PatientName { get; set; }
        public string Status { get; set; }
        public string SupervisorName { get; set; }
        public string LabTechnicianName { get; set; }
        public DateTime createAt { get; set; }
        public List<TestResultDto> Tests { get; set; } = new List<TestResultDto>();
    }
}
