using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace MauiAppSchool.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;
        private readonly JsonSerializerOptions _json;
        private string? _token;

        public string BaseUrl { get; }

        public string WebBase { get; }   // ← host do site MVC (onde estão as fotos)

        // DTOs (mínimos para consumo)
        public record LoginRequest(string Email, string Password);
        public record LoginResponse(string Token, DateTime ExpiresAtUtc);

        public record ProfileVm(string Email, string FullName, string Role, string? ProfilePhoto);
        public record UpdateProfileRequest(string? FullName, string? ProfilePhoto);

        public record CourseDto(int Id, string Name);
        public record SubjectDto(int Id, string Name, int CourseId, int TotalLessons);

        public record MarkDto(int Id, int SubjectId, string Subject, string EvaluationType, float Value, bool? IsPassed, DateTime Date);

        public record AbsenceItem(int Id, int SubjectId, string? Subject, DateTime Date, bool? Justified);
        public record AbsenceSummaryPerSubject(int SubjectId, string? Subject, int Count, double Percentage, int MaxAllowed, bool Exceeded, bool IsExcluded);
        public record AbsenceSummary(IEnumerable<AbsenceSummaryPerSubject> perSubject, object overall);
        public record AbsencesResponse(IEnumerable<AbsenceItem> items, AbsenceSummary summary);

        public record CreateEnrollmentRequestDto(int SubjectId, string? Note);
        public record EnrollmentRequestDto(int Id, int StudentId, int SubjectId, string? Subject, string? Status, DateTime? CreatedAt);

        public record FeedItem(string Type, int Id, int SubjectId, string? Subject, DateTime Date, float? Value, string? EvaluationType, bool? IsPassed);
        public record FeedResponse(DateTime since, int count, IEnumerable<FeedItem> items);

        public record SystemInfoDto(string App, string Version, string? Author, DateTime? BuildDateUtc);
        public record SubjectStatusDto(int SubjectId, string Subject, double? Average, double? PassThreshold, int Absences, int MaxAbsences, bool ExceededAbsences, string Status);

        public ApiService(string baseUrl, string? webBase = null, HttpClient? http = null)
        {
            BaseUrl = baseUrl.TrimEnd('/');
            _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            WebBase = (webBase ?? baseUrl).TrimEnd('/'); // se não passar, usa o da API
        }

        // Token
        public async Task LoadTokenAsync() { try { _token = await SecureStorage.GetAsync("jwt_token"); } catch { _token = null; } SetAuth(_token); }
        public async Task SaveTokenAsync(string? token)
        {
            _token = token; SetAuth(_token);
            if (token is null) SecureStorage.Remove("jwt_token");
            else await SecureStorage.SetAsync("jwt_token", token);
        }
        void SetAuth(string? token) =>
            _http.DefaultRequestHeaders.Authorization = string.IsNullOrWhiteSpace(token) ? null : new AuthenticationHeaderValue("Bearer", token);

        // Helpers
        static StringContent Body<T>(T obj) =>
            new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        async Task<T> Read<T>(HttpResponseMessage resp)
        {
            var url = resp.RequestMessage?.RequestUri?.ToString() ?? "<unknown>";
            var txt = await resp.Content.ReadAsStringAsync();

            if (resp.StatusCode == HttpStatusCode.Unauthorized || resp.StatusCode == HttpStatusCode.Forbidden)
                throw new UnauthorizedAccessException($"{(int)resp.StatusCode} at {url}");

            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"{(int)resp.StatusCode} {resp.ReasonPhrase} at {url}\n{txt}");

            var data = JsonSerializer.Deserialize<T>(txt, _json)
                       ?? throw new InvalidOperationException($"Empty response at {url}");
            return data;
        }
        // Auth
        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var resp = await _http.PostAsync($"{BaseUrl}/api/auth/login", Body(new LoginRequest(email, password)));
            var data = await Read<LoginResponse>(resp);
            await SaveTokenAsync(data.Token);
            return data;
        }
        public Task LogoutAsync() => SaveTokenAsync(null);

        // Público
        public async Task<IReadOnlyList<CourseDto>> GetCoursesAsync()
            => await Read<List<CourseDto>>(await _http.GetAsync($"{BaseUrl}/api/public/courses"));
        public async Task<IReadOnlyList<SubjectDto>> GetSubjectsAsync(int? courseId = null)
        {
            var url = $"{BaseUrl}/api/public/subjects";
            if (courseId.HasValue) url += $"?courseId={courseId.Value}";
            return await Read<List<SubjectDto>>(await _http.GetAsync(url));
        }

        // Perfil
        public async Task<ProfileVm> GetProfileAsync()
            => await Read<ProfileVm>(await _http.GetAsync($"{BaseUrl}/api/students/profile"));
        public async Task UpdateProfileAsync(string? fullName = null, string? profilePhoto = null)
            => await Read<object>(await _http.PutAsync($"{BaseUrl}/api/students/profile", Body(new UpdateProfileRequest(fullName, profilePhoto))));

        // Resolve URL absoluta da foto vinda do site MVC
        // ApiService.cs
        public async Task<Uri?> ResolvePhotoUriAsync(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            raw = raw.Trim().Replace("\\", "/");

            if (Uri.TryCreate(raw, UriKind.Absolute, out var abs)) return abs;

            // já veio com caminho? junta com o host do site
            if (raw.Contains('/'))
            {
                if (raw.StartsWith("~/")) raw = raw[2..];
                if (!raw.StartsWith("/")) raw = "/" + raw;
                return new Uri($"{WebBase}{raw}");
            }

            // só o ficheiro: tenta /uploads/ e depois raiz
            var candidate = $"{WebBase}/uploads/{raw}";
            if (await UrlExistsAsync(candidate)) return new Uri(candidate);

            var fallback = $"{WebBase}/{raw}";
            if (await UrlExistsAsync(fallback)) return new Uri(fallback);

            return null;
        }


        // testa sem baixar o corpo inteiro
        private async Task<bool> UrlExistsAsync(string url)
        {
            try
            {
                // primeiro HEAD (rápido). Se o host bloquear, tenta GET só cabeçalhos.
                using var head = new HttpRequestMessage(HttpMethod.Head, url);
                using var r1 = await _http.SendAsync(head, HttpCompletionOption.ResponseHeadersRead);
                if ((int)r1.StatusCode >= 200 && (int)r1.StatusCode < 300) return true;

                using var get = new HttpRequestMessage(HttpMethod.Get, url);
                using var r2 = await _http.SendAsync(get, HttpCompletionOption.ResponseHeadersRead);
                if (!r2.IsSuccessStatusCode) return false;

                var ct = r2.Content.Headers.ContentType?.MediaType;
                return ct != null && ct.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
            }
            catch
            {
                return false;
            }
        }



        // Notas / Faltas
        public async Task<IReadOnlyList<MarkDto>> GetMarksAsync(int? subjectId = null)
        {
            var url = $"{BaseUrl}/api/marks";
            if (subjectId.HasValue) url += $"?subjectId={subjectId.Value}";
            return await Read<List<MarkDto>>(await _http.GetAsync(url));
        }
        public async Task<AbsencesResponse> GetAbsencesAsync()
            => await Read<AbsencesResponse>(await _http.GetAsync($"{BaseUrl}/api/absences"));

        // Pedidos
        public async Task CreateEnrollmentRequestAsync(int subjectId, string? note = null)
            => await Read<object>(await _http.PostAsync($"{BaseUrl}/api/enrollment-requests", Body(new CreateEnrollmentRequestDto(subjectId, note))));
        public async Task<IReadOnlyList<EnrollmentRequestDto>> GetMyEnrollmentRequestsAsync()
            => await Read<List<EnrollmentRequestDto>>(await _http.GetAsync($"{BaseUrl}/api/enrollment-requests/my"));

        // Feed / Status / Info
        public async Task<FeedResponse> GetFeedAsync(DateTime? since = null)
        {
            var url = $"{BaseUrl}/api/feed";
            if (since.HasValue) url += $"?since={since.Value.ToUniversalTime():O}";
            return await Read<FeedResponse>(await _http.GetAsync(url));
        }
        public async Task<IReadOnlyList<SubjectStatusDto>> GetStatusPerSubjectAsync()
            => await Read<List<SubjectStatusDto>>(await _http.GetAsync($"{BaseUrl}/api/status/per-subject"));
        public async Task<SystemInfoDto> GetSystemInfoAsync()
            => await Read<SystemInfoDto>(await _http.GetAsync($"{BaseUrl}/api/system/info"));


    
        public record ForgotPasswordRequest(string Email);
        public record ResetPasswordRequest(string Email, string Token, string NewPassword);

   
        public async Task ForgotPasswordAsync(string email)
        {
            var resp = await _http.PostAsync($"{BaseUrl}/api/auth/forgot-password",
                                             Body(new ForgotPasswordRequest(email)));
            // backend pode devolver 200 mesmo que email não exista (por segurança)
            _ = await Read<object>(resp);
        }

        public async Task ResetPasswordAsync(string email, string token, string newPassword)
        {
            var resp = await _http.PostAsync($"{BaseUrl}/api/auth/reset-password",
                                             Body(new ResetPasswordRequest(email, token, newPassword)));
            _ = await Read<object>(resp);
        }

        // opcional: login anónimo = limpa token
        public Task UseAnonymousAsync() => SaveTokenAsync(null);

    }
}
