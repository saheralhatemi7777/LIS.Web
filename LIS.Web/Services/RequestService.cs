using Newtonsoft.Json;
using مشروع_ادار_المختبرات.DTOS;

namespace مشروع_ادار_المختبرات.Services
{
    

    public class RequestService
    {
        private readonly HttpClient _httpClient;

        public RequestService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<RequestDto>> GetRequestsAsync()
        {
            var response = await _httpClient.GetAsync("https://localhost:7116/api/RequestTest");
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            var result = JsonConvert.DeserializeObject<List<RequestDto>>(json);

            return result ?? new List<RequestDto>();
        }
    }

}
