public class RequestTestDto
{
    public int RequestTestID { get; set; }
    public int RequestID { get; set; }
    public string PatientName { get; set; }
    public string Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public List<TestDto> Tests { get; set; }
}

public class TestDto
{
    public string Name { get; set; }
    public string Samples { get; set; }
    public decimal Price { get; set; }
}
