namespace مشروع_ادار_المختبرات.DTOS
{
    
        public class RequestViewDto
        {
            public int RequestTestID { get; set; }
            public int RequestID { get; set; }
            public string PatientName { get; set; }
            public string Status { get; set; }
            public DateTime CreatedAt { get; set; }
            public int LabTechniciansUserID { get; set; }

           public List<TestViewDto> Tests { get; set; }
        }

        public class TestViewDto
        {
            public int TestId { get; set; }
            public string Name { get; set; }
            public string Samples { get; set; }
            public decimal Price { get; set; }
            public string Roung { get; set; }

        }
}
