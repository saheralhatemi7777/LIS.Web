using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using مشروع_ادار_المختبرات.Models;
using مشروع_ادار_المختبرات.DTOS;

public class UserApiService
{
    private readonly HttpClient _httpClient;

    public UserApiService(IHttpClientFactory httpClientFactory)
    {
        _httpClient = httpClientFactory.CreateClient("UserApi");
    }

    public async Task<List<DTOEditUsers>> GetUsersAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<DTOEditUsers>>("api/Users");
    }
}
