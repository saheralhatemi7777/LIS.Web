using Newtonsoft.Json;

public class ResultData
{
    [JsonProperty("resultId")]
    public int ResultId { get; set; }

    [JsonProperty("resultValue")]
    public string? Value { get; set; }

    [JsonProperty("createdAt")]
    public DateTime? CreatedAt { get; set; }
}

public class TestData
{
    [JsonProperty("testId")]
    public int Id { get; set; }

    [JsonProperty("testNameEn")]
    public string EnglishName { get; set; } = string.Empty;

    [JsonProperty("testNameAr")]
    public string ArabicName { get; set; } = string.Empty;

    [JsonProperty("sampleType")]
    public string SampleType { get; set; } = string.Empty;

    [JsonProperty("normalRange")]
    public string NormalRange { get; set; } = string.Empty;

    [JsonProperty("testPrice")]
    public decimal Price { get; set; }

    [JsonProperty("resultValue")]
    public string? ResultValue { get; set; }

    [JsonProperty("results")]
    public List<ResultData> Results { get; set; } = new();
}

public class RequestData
{
    [JsonProperty("requestId")]
    public int Id { get; set; }

    [JsonProperty("requestDate")]
    public DateTime Date { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("tests")]
    public List<TestData> Tests { get; set; } = new();
}

public class RecordData
{
    [JsonProperty("recordId")]
    public int Id { get; set; }

    [JsonProperty("requestDate")]
    public DateTime Date { get; set; }

    [JsonProperty("status")]
    public string? Status { get; set; }

    [JsonProperty("requests")]
    public List<RequestData> Requests { get; set; } = new();
}

public class PatientData
{
    [JsonProperty("patientId")]
    public int Id { get; set; }

    [JsonProperty("fullName")]
    public string FullName { get; set; } = string.Empty;

    [JsonProperty("phone")]
    public string Phone { get; set; } = string.Empty;

    [JsonProperty("address")]
    public string Address { get; set; } = string.Empty;

    [JsonProperty("username")]
    public string Username { get; set; } = string.Empty;

    [JsonProperty("records")]
    public List<RecordData> Records { get; set; } = new();
}
