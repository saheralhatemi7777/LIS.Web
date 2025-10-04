public class SaveTestResultRequestDto
{
    public int RequestTestID { get; set; }
    public List<int> TestId { get; set; } = new();
    public List<string> ResultValue { get; set; } = new();
    public DateTime CreatedAt { get; set; }
    public int LabTechniciansUserID { get; set; }
    public int Requestid { get; set; }

}
