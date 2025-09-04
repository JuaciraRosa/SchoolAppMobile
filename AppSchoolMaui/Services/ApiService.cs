using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace AppSchoolMaui.Services
{
    public sealed class ApiService
    {
        // ==== Config ====
        // Ajuste aqui para os seus domínios
        public string BaseUrl { get; } = "https://escolainfosysapi.somee.com"; // API
        public string WebBase { get; } = "https://escolainfosys.somee.com";    // MVC (site)

        private readonly HttpClient _http = new();
        private readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };

        public ApiService()
        {
            _http.Timeout = TimeSpan.FromSeconds(30);
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

           
        }

        public async Task BootstrapAuthAsync()
        {
            var token = await SecureStorage.GetAsync("token");
            SetAuth(token);
        }

        private void SetAuth(string? token)
        {
            _http.DefaultRequestHeaders.Authorization =
                string.IsNullOrWhiteSpace(token) ? null : new AuthenticationHeaderValue("Bearer", token);
        }

        private static StringContent Body(object obj) =>
            new StringContent(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        private async Task<T> Read<T>(HttpResponseMessage resp)
        {
            var url = resp.RequestMessage?.RequestUri?.ToString() ?? "?";
            var txt = await resp.Content.ReadAsStringAsync();
            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"{(int)resp.StatusCode} {resp.ReasonPhrase} at {url}\n{txt}");

            return JsonSerializer.Deserialize<T>(txt, _json)!;
        }

        // ========= Auth =========

        public record LoginReq(string Email, string Password);
        public record LoginResp(string Token, DateTime ExpiresAtUtc);

        public async Task LoginAsync(string email, string password)
        {
            var resp = await _http.PostAsync($"{BaseUrl}/api/auth/login", Body(new LoginReq(email, password)));
            var data = await Read<LoginResp>(resp);

            await SecureStorage.SetAsync("token", data.Token);
            SetAuth(data.Token);
        }

        public async Task LogoutAsync()
        {
            SecureStorage.Remove("token");
            SetAuth(null);
            await Task.CompletedTask;
        }

        // aciona endpoint que envia e-mail e devolve (opcionalmente) o link
        public record ForgotPasswordReq(string Email);
        public record ForgotPasswordResp(string? link);

        public async Task ForgotAsync(string email)
        {
            var resp = await _http.PostAsync($"{BaseUrl}/api/auth/forgot-password", Body(new ForgotPasswordReq(email)));
            // ok sem corpo? não tem problema; se tiver 'link', abrimos
            if (resp.IsSuccessStatusCode)
            {
                var txt = await resp.Content.ReadAsStringAsync();
                if (!string.IsNullOrWhiteSpace(txt))
                {
                    var data = JsonSerializer.Deserialize<ForgotPasswordResp>(txt, _json);
                    if (!string.IsNullOrWhiteSpace(data?.link))
                        await Browser.OpenAsync(data.link, BrowserLaunchMode.External);
                }
            }
            else
            {
                // Deixa a exceção detalhada se houver erro real
                await Read<object>(resp);
            }
        }

        // Só abre a página do site (sem usar a API)
        public Task ForgotOnWebAsync(string? email = null)
        {
            var url = string.IsNullOrWhiteSpace(email)
                ? $"{WebBase}/Account/ForgotPassword"
                : $"{WebBase}/Account/ForgotPassword?email={Uri.EscapeDataString(email)}";

            return Browser.OpenAsync(new Uri(url), BrowserLaunchMode.External);
        }

        // ========= Profile =========

        public record MeDto(string Email, string FullName, string Role, string? ProfilePhoto);

        public async Task<MeDto> GetProfileAsync()
        {
            var resp = await _http.GetAsync($"{BaseUrl}/api/students/profile");
            return await Read<MeDto>(resp);
        }

        // Atualiza nome e/ou URL da foto
        public record UpdateProfileReq(string? FullName, string? ProfilePhoto);

        public async Task<MeDto> UpdateProfileAsync(string? fullName, string? profilePhoto)
        {
            var resp = await _http.PutAsync($"{BaseUrl}/api/students/profile", Body(new UpdateProfileReq(fullName, profilePhoto)));
            if (resp.StatusCode == HttpStatusCode.NoContent)
                return await GetProfileAsync();

            return await Read<MeDto>(resp);
        }

        // Upload foto
        public record UploadPhotoResp(string path, string url);

        public async Task<UploadPhotoResp> UploadStudentProfilePhotoAsync(byte[] bytes, string fileName)
        {
            using var form = new MultipartFormDataContent();
            var file = new ByteArrayContent(bytes);
            file.Headers.ContentType = new MediaTypeHeaderValue(GetMime(fileName));
            form.Add(file, "file", fileName);

            var resp = await _http.PostAsync($"{BaseUrl}/api/students/profile/photo", form);
            return await Read<UploadPhotoResp>(resp);
        }

        // Converte string (fname / caminho relativo / url absoluta) em URI absolute
        public Task<Uri?> ResolvePhotoUriAsync(string? pathOrUrl)
        {
            if (string.IsNullOrWhiteSpace(pathOrUrl))
                return Task.FromResult<Uri?>(null);

            if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var abs))
                return Task.FromResult<Uri?>(abs);

            // se só veio o nome do arquivo, API serve em /uploads/{fname}
            var rel = pathOrUrl.TrimStart('/');
            var url = $"{BaseUrl}/uploads/{rel}";
            return Task.FromResult<Uri?>(new Uri(url));
        }

        private static string GetMime(string fileName)
        {
            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            return ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
        }

        // ========= Público =========

        public record CourseDto(int Id, string Name);
        public record SubjectDto(int Id, string Name, int CourseId, int TotalLessons);

        public async Task<List<CourseDto>> GetPublicCoursesAsync()
        {
            var resp = await _http.GetAsync($"{BaseUrl}/api/public/courses");
            return await Read<List<CourseDto>>(resp);
        }
        public async Task<List<SubjectDto>> GetPublicSubjectsAsync(int? courseId = null)
        {
            var url = $"{BaseUrl}/api/public/subjects";
            if (courseId is not null) url += $"?courseId={courseId.Value}";
            var resp = await _http.GetAsync(url);
            return await Read<List<SubjectDto>>(resp);
        }

        // ========= Notas =========

        public record MarkDto(int Id, int SubjectId, string Subject, string EvaluationType,
                              float Value, bool? IsPassed, DateTime Date);

        public async Task<List<MarkDto>> GetMarksAsync(int? subjectId = null)
        {
            var url = $"{BaseUrl}/api/marks";
            if (subjectId is not null) url += $"?subjectId={subjectId.Value}";
            var resp = await _http.GetAsync(url);
            return await Read<List<MarkDto>>(resp);
        }

        public async Task<Dictionary<int, double>> GetMyAveragesAsync()
        {
            var resp = await _http.GetAsync($"{BaseUrl}/api/marks/averages");
            return await Read<Dictionary<int, double>>(resp);
        }

        // ========= Faltas =========

        public record AbsenceItemDto(int Id, int SubjectId, string? Subject, DateTime Date, bool Justified);
        public record AbsencePerSubjectDto(int SubjectId, string? Subject, int Count, double Percentage,
                                           int MaxAllowed, bool Exceeded, bool IsExcluded);
        public record AbsenceOverallDto(bool AnyExcluded, bool AnyExceeded);
        public record AbsenceResponse(List<AbsenceItemDto> items, AbsenceSummary summary);
        public record AbsenceSummary(List<AbsencePerSubjectDto> perSubject, AbsenceOverallDto overall);

        public async Task<AbsenceResponse> GetAbsencesAsync()
        {
            var resp = await _http.GetAsync($"{BaseUrl}/api/absences");
            return await Read<AbsenceResponse>(resp);
        }

        // ========= Status =========

        public record StatusDto(int SubjectId, string Subject, double? Average, double? PassThreshold,
                                int Absences, int MaxAbsences, bool ExceededAbsences, string Status);

        public async Task<List<StatusDto>> GetStatusAsync()
        {
            var resp = await _http.GetAsync($"{BaseUrl}/api/status/per-subject");
            return await Read<List<StatusDto>>(resp);
        }

        // ========= Feed =========

        public record FeedItem(string Type, int Id, int SubjectId, string? Subject,
                               DateTime Date, float? Value, string? EvaluationType, bool? IsPassed);
        public record FeedResponse(DateTime since, int count, List<FeedItem> items);

        public async Task<FeedResponse> GetFeedAsync(DateTime? sinceUtc = null)
        {
            var url = $"{BaseUrl}/api/feed";
            if (sinceUtc is not null)
                url += $"?since={Uri.EscapeDataString(sinceUtc.Value.ToUniversalTime().ToString("o"))}";

            var resp = await _http.GetAsync(url);
            return await Read<FeedResponse>(resp);
        }

        // polling simples para avisos (usado após login)
        private System.Timers.Timer? _timer;
        private DateTime? _lastSince;

        public void StartFeedPolling(TimeSpan interval, Func<List<FeedItem>, Task> onItems)
        {
            _timer?.Stop();
            _timer?.Dispose();
            _lastSince = DateTime.UtcNow.AddDays(-1);

            _timer = new System.Timers.Timer(interval.TotalMilliseconds) { AutoReset = true };
            _timer.Elapsed += async (_, __) =>
            {
                try
                {
                    var data = await GetFeedAsync(_lastSince);
                    if (data.items.Count > 0)
                    {
                        _lastSince = data.items.Max(i => i.Date).ToUniversalTime();
                        await onItems(data.items);
                    }
                }
                catch { /* silencia polling */ }
            };
            _timer.Start();
        }

        public void StopFeedPolling()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
            _lastSince = null;
        }

        // ========= Enrollment Requests =========

        public record EnrollmentRequestDto(int Id, int StudentId, int SubjectId, string? Subject,
                                           string? Status, DateTime? CreatedAt);
        public record CreateEnrollmentReq(int SubjectId, string? Note);

        public async Task<List<EnrollmentRequestDto>> GetMyEnrollmentRequestsAsync()
        {
            var resp = await _http.GetAsync($"{BaseUrl}/api/enrollment-requests/my");
            return await Read<List<EnrollmentRequestDto>>(resp);
        }

        public async Task CreateEnrollmentRequestAsync(int subjectId, string? note)
        {
            var resp = await _http.PostAsync($"{BaseUrl}/api/enrollment-requests",
                Body(new CreateEnrollmentReq(subjectId, note)));
            await Read<object>(resp); // valida erro se houver; Created 201 não tem corpo
        }

     
    }
}
