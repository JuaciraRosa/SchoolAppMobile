using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace SchoolAppMobile.Services
{
    public static class AuthService
    {
        public static async Task<AuthResult> LoginAsync(string email, string password)
        {
            try
            {
                var client = new HttpClient();
                var payload = new { email, password };
                var json = JsonSerializer.Serialize(payload);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await client.PostAsync($"{ApiConstants.BaseUrl}/auth/login", content);

                if (response.IsSuccessStatusCode)
                {
                    var body = await response.Content.ReadAsStringAsync();
                    var result = JsonSerializer.Deserialize<AuthSuccessResponse>(body, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    return new AuthResult
                    {
                        IsSuccess = true,
                        Token = result.Token
                    };
                }

                return new AuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = "Invalid credentials"
                };
            }
            catch (Exception ex)
            {
                return new AuthResult
                {
                    IsSuccess = false,
                    ErrorMessage = $"Error: {ex.Message}"
                };
            }
        }
    }
}
