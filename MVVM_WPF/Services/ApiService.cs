using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using MVVM_WPF.Model; 

namespace MVVM_WPF.Services
{
    public class ApiService
    {
        private static readonly HttpClient _client = new HttpClient();
        private const string BaseUrl = "https://localhost:7271/api";

        private void PrepareAuthorizationHeader()
        {
            if (!string.IsNullOrEmpty(TokenStore.Token))
            {
                _client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", TokenStore.Token);
            }
            else
            {
                _client.DefaultRequestHeaders.Authorization = null;
            }
        }

        public async Task<string> LoginAsync(string username, string password)
        {
            var loginDto = new { Username = username, Password = password };
            HttpResponseMessage response = await _client.PostAsJsonAsync($"{BaseUrl}/auth/login", loginDto);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return result?.Token;
            }
            throw new Exception("Invalid username or password");
        }

        public async Task<List<TaskModel>> GetTasksAsync(DataSourceRequest payload)
        {
            PrepareAuthorizationHeader();
            HttpResponseMessage response = await _client.PostAsJsonAsync($"{BaseUrl}/tasks/filter-dynamic", payload);

            if (!response.IsSuccessStatusCode)
            {
                string errorText = await response.Content.ReadAsStringAsync();
                throw new Exception($"API connection error: {response.StatusCode}\n{errorText}");
            }

            string jsonText = await response.Content.ReadAsStringAsync();
            var options = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            jsonText = jsonText.TrimStart();

            if (jsonText.StartsWith("{"))
            {
                using (var doc = JsonDocument.Parse(jsonText))
                {
                    var root = doc.RootElement;
                    if (root.TryGetProperty("data", out var dataElement) ||
                        root.TryGetProperty("Data", out dataElement) ||
                        root.TryGetProperty("items", out dataElement) ||
                        root.TryGetProperty("Items", out dataElement))
                    {
                        return JsonSerializer.Deserialize<List<TaskModel>>(dataElement.GetRawText(), options);
                    }
                }
            }
            else if (jsonText.StartsWith("["))
            {
                return JsonSerializer.Deserialize<List<TaskModel>>(jsonText, options);
            }

            throw new Exception("API returnd successfuly but format is wrong.");
        }
        public async Task<TaskModel> AddTaskAsync(TaskModel newTask)
        {
            PrepareAuthorizationHeader();
            HttpResponseMessage response = await _client.PostAsJsonAsync($"{BaseUrl}/tasks", newTask);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Adding failed: {response.StatusCode}\n{error}");
            }

            return await response.Content.ReadFromJsonAsync<TaskModel>(new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        }

        public async Task UpdateTaskAsync(TaskModel updatedTask)
        {
            PrepareAuthorizationHeader();
            HttpResponseMessage response = await _client.PutAsJsonAsync($"{BaseUrl}/tasks/{updatedTask.Id}", updatedTask);

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Update failed: {response.StatusCode}\n{error}");
            }
        }

        public async Task DeleteTaskAsync(string taskId)
        {
            PrepareAuthorizationHeader();
            HttpResponseMessage response = await _client.DeleteAsync($"{BaseUrl}/tasks/{taskId}");

            if (!response.IsSuccessStatusCode)
            {
                string error = await response.Content.ReadAsStringAsync();
                throw new Exception($"Delete failed: {response.StatusCode}\n{error}");
            }
        }
    }
}