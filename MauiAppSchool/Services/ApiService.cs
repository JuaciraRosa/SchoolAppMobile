using System;
using System.Collections.Generic;
using System.Linq;
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

        public ApiService(string baseUrl, HttpClient? http = null)
        {
            BaseUrl = baseUrl.TrimEnd('/');
            _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            _json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
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
        static StringContent Body<T>(T obj) => new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");
        async Task<T> Read<T>(HttpResponseMessage resp)
        {
            var txt = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode) throw new HttpRequestException($"{(int)resp.StatusCode} {resp.ReasonPhrase}: {txt}");
            return JsonSerializer.Deserialize<T>(txt, _json) ?? throw new InvalidOperationException("Empty response.");
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
    }
}
