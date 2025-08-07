using SchoolAppMobile.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SchoolAppMobile.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public ApiService()
        {
            _httpClient = new HttpClient
            {
                BaseAddress = new Uri("https://www.escolainfosysapi.somee.com/api/") 
            };

            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
        }

        private async Task AddJwtHeaderAsync()
        {
            var token = await GetToken();
            _httpClient.DefaultRequestHeaders.Authorization = null; // limpa qualquer valor anterior

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", token);
            }
        }


        public async Task<T?> GetAsync<T>(string endpoint)
        {
            await AddJwtHeaderAsync();
            var response = await _httpClient.GetAsync(endpoint);
            var body = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"GET {endpoint} => {response.StatusCode}");
            Console.WriteLine("Response: " + body);

            if (!response.IsSuccessStatusCode) return default;
            return JsonSerializer.Deserialize<T>(body, _jsonOptions);
        }

        public async Task<List<T>> GetListAsync<T>(string endpoint)
        {
            await AddJwtHeaderAsync();
            var response = await _httpClient.GetAsync(endpoint);
            if (!response.IsSuccessStatusCode) return new List<T>();

            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<T>>(json, _jsonOptions) ?? new List<T>();
        }

        public async Task<bool> PostAsync<T>(string endpoint, T data)
        {
            await AddJwtHeaderAsync();

            var json = JsonSerializer.Serialize(data, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            HttpResponseMessage response;

            try
            {
                response = await _httpClient.PostAsync(endpoint, content);
            }
            catch (Exception ex)
            {
                // Mostra erro genérico de rede (sem console)
                await Shell.Current.DisplayAlert("Erro", "Erro de rede: " + ex.Message, "OK");
                return false;
            }

            var responseBody = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
            {
                //  Mostra erro vindo da API
                await Shell.Current.DisplayAlert("Erro", $"API retornou {response.StatusCode}\n{responseBody}", "OK");
                return false;
            }

            return true;
        }


        public Task<string?> GetToken()
        {
            return Task.FromResult(Preferences.Get("jwt_token", null));
        }

        public async Task<string?> LoginAsync(LoginDto loginDto)
        {
            var json = JsonSerializer.Serialize(loginDto, _jsonOptions);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("auth/login", content);

            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine(" Login falhou com status: " + response.StatusCode);
                return null;
            }

            var responseBody = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(responseBody);

            if (doc.RootElement.TryGetProperty("token", out var tokenElement))
            {
                var token = tokenElement.GetString();
                if (!string.IsNullOrEmpty(token))
                {
                    Preferences.Set("jwt_token", token); // salva localmente
                    Debug.WriteLine(" JWT salvo: " + token); // <-- AQUI
                    return token;
                }
            }

            Debug.WriteLine("Token não encontrado na resposta.");
            return null;
        }

        public async Task<bool> PostRawAsync(string endpoint, string rawString)
        {
            await AddJwtHeaderAsync();
            var json = JsonSerializer.Serialize(rawString);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(endpoint, content);
            var responseBody = await response.Content.ReadAsStringAsync();

            Debug.WriteLine($" POST {endpoint} => {(int)response.StatusCode} {response.StatusCode}");
            Debug.WriteLine(" Enviado: " + json);
            Debug.WriteLine(" Recebido: " + responseBody);

            return response.IsSuccessStatusCode;
        }

        public async Task<bool> UpdateStudentProfileAsync(string newUsername, FileResult? photoFile)
        {
            await AddJwtHeaderAsync(); // adiciona token JWT no cabeçalho

            using var content = new MultipartFormDataContent();
            content.Add(new StringContent(newUsername), "UserName");

            if (photoFile != null)
            {
                try
                {
                    var stream = await photoFile.OpenReadAsync();
                    var fileContent = new StreamContent(stream);

                    // Define dinamicamente o tipo MIME com base na extensão
                    var extension = Path.GetExtension(photoFile.FileName).ToLower();
                    var contentType = extension switch
                    {
                        ".jpg" or ".jpeg" => "image/jpeg",
                        ".png" => "image/png",
                        ".gif" => "image/gif",
                        _ => "application/octet-stream" // fallback
                    };

                    fileContent.Headers.ContentType = new MediaTypeHeaderValue(contentType);
                    content.Add(fileContent, "ProfilePhoto", Path.GetFileName(photoFile.FileName));
                }
                catch (Exception ex)
                {
                    await Shell.Current.DisplayAlert("Erro", $"Erro ao carregar imagem: {ex.Message}", "OK");
                    return false;
                }
            }

            try
            {
                var response = await _httpClient.PutAsync("students/profile", content);
                var responseText = await response.Content.ReadAsStringAsync();

                Debug.WriteLine(" Update profile response: " + responseText);

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Erro", $"Erro ao atualizar perfil: {ex.Message}", "OK");
                return false;
            }
        }

        public async Task<bool> ForgotPasswordAsync(string email)
        {
            var dto = new ForgotPasswordDto { Email = email };

            var response = await _httpClient.PostAsJsonAsync("auth/forgot-password", dto);
            return response.IsSuccessStatusCode;
        }

        public async Task<bool> ResetPasswordAsync(string email, string token, string newPassword)
        {
            var payload = new
            {
                Email = email,
                Token = token,
                NewPassword = newPassword
            };

            var json = JsonSerializer.Serialize(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("auth/reset-password", content);
            return response.IsSuccessStatusCode;
        }


        public async Task<bool> ChangePasswordAsync(ChangePasswordDto dto)
        {
            await AddJwtHeaderAsync();

            try
            {
                var response = await _httpClient.PostAsJsonAsync("auth/change-password", dto);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }








    }


}
