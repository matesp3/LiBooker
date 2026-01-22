using LiBooker.Blazor.Client.Models;
using LiBooker.Shared.DTOs;

namespace LiBookerWasmApp.Services.Clients;

public class PersonClient(HttpClient http) : ICustomClient
{
    private readonly HttpClient _http = http;

    public async Task<ApiResponse<List<Person>?>> GetAllAsync(CancellationToken ct = default)
    {
        var requestUrl = "api/persons";
        return await ApiClient<List<Person>?>.GetJsonAsync(requestUrl, _http, ct);
    }

    public async Task<ApiResponse<Person?>> GetPersonByIdAsync(int id, CancellationToken ct = default)
    {
        var requestUrl = $"api/persons/{id}";
        return await ApiClient<Person?>.GetJsonAsync(requestUrl, _http, ct);
    }

    public async Task<ApiResponse<PersonUpdate?>> UpdatePersonAsync(int id, PersonUpdate dto, CancellationToken ct = default)
    {
        var requestUrl = $"api/persons/edit/{id}";
        return await ApiClient<PersonUpdate?>.PutJsonAsync(requestUrl, dto, _http, ct);
    }
}