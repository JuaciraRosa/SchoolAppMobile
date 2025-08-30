using Microsoft.AspNetCore.SignalR.Client;
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
        HubConnection? _hub;
        public event EventHandler<GradeNotification>? GradePushed;
        public event EventHandler<StatusNotification>? StatusPushed;

        private CancellationTokenSource? _feedCts;
        private DateTime _feedSince = DateTime.UtcNow;

        public record GradeNotification(int SubjectId, string? Subject, float Value, string? EvaluationType, DateTime Date);
        public record StatusNotification(int SubjectId, string Subject, string Status, double? Average);

        public string BaseUrl { get; }
        public string WebBase { get; }   // domínio do site (MVC) — p/ abrir páginas e resolver fotos

        // ===== DTOs mínimas =====
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

        // Forgot/Reset DTOs
        public record ForgotPasswordReq(string Email);
        public record ResetPasswordReq(string Email, string? Code, string? Token, string NewPassword);

        public ApiService(string baseUrl, string? webBase = null, HttpClient? http = null)
        {
            BaseUrl = baseUrl.TrimEnd('/');
            WebBase = (webBase ?? baseUrl).TrimEnd('/');

            _http = http ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

            _json = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
        }

        // ===== Token =====
        public async Task LoadTokenAsync()
        {
            try { _token = await SecureStorage.GetAsync("jwt_token"); }
            catch { _token = null; }
            SetAuth(_token);
        }

        public async Task SaveTokenAsync(string? token)
        {
            _token = token;
            SetAuth(_token);
            if (token is null) SecureStorage.Remove("jwt_token");
            else await SecureStorage.SetAsync("jwt_token", token);
        }

        void SetAuth(string? token) =>
            _http.DefaultRequestHeaders.Authorization =
                string.IsNullOrWhiteSpace(token) ? null : new AuthenticationHeaderValue("Bearer", token);

        public Task UseAnonymousAsync() => SaveTokenAsync(null);

        // ===== Helpers =====
        private static StringContent Body<T>(T obj) =>
            new(JsonSerializer.Serialize(obj), Encoding.UTF8, "application/json");

        private async Task<T> Read<T>(HttpResponseMessage resp)
        {
            var url = resp.RequestMessage?.RequestUri?.ToString() ?? "<unknown>";
            var txt = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"{(int)resp.StatusCode} {resp.ReasonPhrase} at {url}\n{txt}");

            var data = JsonSerializer.Deserialize<T>(txt, _json);
            if (data is null)
                throw new InvalidOperationException($"Empty response at {url}");

            return data;
        }

        // ===== Auth =====
        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var resp = await _http.PostAsync($"{BaseUrl}/api/auth/login",
                Body(new LoginRequest(email, password)));
            var data = await Read<LoginResponse>(resp);
            await SaveTokenAsync(data.Token);
            return data;
        }

        public Task LogoutAsync() => SaveTokenAsync(null);

        // Forgot (envia email) — simples
        public async Task ForgotPasswordAsync(string email)
        {
            var resp = await _http.PostAsync($"{BaseUrl}/api/auth/forgot-password",
                Body(new ForgotPasswordReq(email)));
            if (!resp.IsSuccessStatusCode)
            {
                var txt = await resp.Content.ReadAsStringAsync();
                throw new HttpRequestException($"{(int)resp.StatusCode} {resp.ReasonPhrase}: {txt}");
            }
        }

        // Forgot (tenta extrair link, se a API devolver {link})
        public async Task<string?> ForgotPasswordGetLinkAsync(string email)
        {
            var resp = await _http.PostAsync($"{BaseUrl}/api/auth/forgot-password",
                Body(new ForgotPasswordReq(email)));
            var txt = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"{(int)resp.StatusCode} {resp.ReasonPhrase}: {txt}");

            try
            {
                using var doc = JsonDocument.Parse(txt);
                return doc.RootElement.TryGetProperty("link", out var l) ? l.GetString() : null;
            }
            catch { return null; }
        }

        // Reset 100% na app — usa o "code" (Base64Url) do email
        public async Task ResetPasswordAsync(string email, string code, string newPassword)
        {
            var body = new ResetPasswordReq(email, Code: code, Token: null, NewPassword: newPassword);
            var resp = await _http.PostAsync($"{BaseUrl}/api/auth/reset-password", Body(body));
            if (!resp.IsSuccessStatusCode)
            {
                var txt = await resp.Content.ReadAsStringAsync();
                throw new HttpRequestException($"{(int)resp.StatusCode} {resp.ReasonPhrase}: {txt}");
            }
        }

        // ===== Público =====
        public async Task<IReadOnlyList<CourseDto>> GetCoursesAsync() =>
            await Read<List<CourseDto>>(await _http.GetAsync($"{BaseUrl}/api/public/courses"));

        public async Task<IReadOnlyList<SubjectDto>> GetSubjectsAsync(int? courseId = null)
        {
            var url = $"{BaseUrl}/api/public/subjects";
            if (courseId.HasValue) url += $"?courseId={courseId.Value}";
            return await Read<List<SubjectDto>>(await _http.GetAsync(url));
        }

        // ===== Perfil =====
        public async Task<ProfileVm> GetProfileAsync() =>
            await Read<ProfileVm>(await _http.GetAsync($"{BaseUrl}/api/students/profile"));

        // Importante: não tenta desserializar 200 OK vazio
        public async Task UpdateProfileAsync(string? fullName = null, string? profilePhoto = null)
        {
            var resp = await _http.PutAsync($"{BaseUrl}/api/students/profile",
                Body(new UpdateProfileRequest(fullName, profilePhoto)));

            if (!resp.IsSuccessStatusCode)
            {
                var txt = await resp.Content.ReadAsStringAsync();
                throw new HttpRequestException($"{(int)resp.StatusCode} {resp.ReasonPhrase}: {txt}");
            }
        }

        // Upload de foto p/ API (endpoint: POST /api/students/profile/photo)
        // Retorna (path relativo e url absoluta) conforme resposta da API.
        public async Task<(string path, string url)> UploadStudentProfilePhotoAsync(byte[] bytes, string fileName)
        {
            using var form = new MultipartFormDataContent();

            var fileContent = new ByteArrayContent(bytes);
            var ext = System.IO.Path.GetExtension(fileName).ToLowerInvariant();
            var mime = ext switch
            {
                ".jpg" or ".jpeg" => "image/jpeg",
                ".png" => "image/png",
                ".gif" => "image/gif",
                ".webp" => "image/webp",
                _ => "application/octet-stream"
            };
            fileContent.Headers.ContentType = new MediaTypeHeaderValue(mime);

            // O NOME DO CAMPO deve ser "file" (conforme controller)
            form.Add(fileContent, "file", fileName);

            var resp = await _http.PostAsync($"{BaseUrl}/api/students/profile/photo", form);
            var txt = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
                throw new HttpRequestException($"{(int)resp.StatusCode} {resp.ReasonPhrase}: {txt}");

            using var doc = JsonDocument.Parse(txt);
            var root = doc.RootElement;
            var path = root.GetProperty("path").GetString()!;
            var url = root.GetProperty("url").GetString()!;
            return (path, url);
        }

        // Resolver URL absoluta da foto (prioriza /uploads do WebBase)
        public async Task<Uri?> ResolvePhotoUriAsync(string? raw)
        {
            if (string.IsNullOrWhiteSpace(raw)) return null;
            raw = raw.Trim().Replace("\\", "/");

            // já absoluta
            if (Uri.TryCreate(raw, UriKind.Absolute, out var abs)) return abs;

            // já veio com pasta? junta com WebBase
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

        private async Task<bool> UrlExistsAsync(string url)
        {
            try
            {
                using var head = new HttpRequestMessage(HttpMethod.Head, url);
                using var r1 = await _http.SendAsync(head, HttpCompletionOption.ResponseHeadersRead);
                if ((int)r1.StatusCode >= 200 && (int)r1.StatusCode < 300) return true;

                using var get = new HttpRequestMessage(HttpMethod.Get, url);
                using var r2 = await _http.SendAsync(get, HttpCompletionOption.ResponseHeadersRead);
                if (!r2.IsSuccessStatusCode) return false;

                var ct = r2.Content.Headers.ContentType?.MediaType;
                return ct != null && ct.StartsWith("image/", StringComparison.OrdinalIgnoreCase);
            }
            catch { return false; }
        }

        // ===== Notas / Faltas =====
        public async Task<IReadOnlyList<MarkDto>> GetMarksAsync(int? subjectId = null)
        {
            var url = $"{BaseUrl}/api/marks";
            if (subjectId.HasValue) url += $"?subjectId={subjectId.Value}";
            return await Read<List<MarkDto>>(await _http.GetAsync(url));
        }

        public async Task<AbsencesResponse> GetAbsencesAsync() =>
            await Read<AbsencesResponse>(await _http.GetAsync($"{BaseUrl}/api/absences"));

        // ===== Pedidos =====
        public async Task CreateEnrollmentRequestAsync(int subjectId, string? note = null) =>
            await Read<object>(await _http.PostAsync(
                $"{BaseUrl}/api/enrollment-requests",
                Body(new CreateEnrollmentRequestDto(subjectId, note))));

        public async Task<IReadOnlyList<EnrollmentRequestDto>> GetMyEnrollmentRequestsAsync() =>
            await Read<List<EnrollmentRequestDto>>(await _http.GetAsync($"{BaseUrl}/api/enrollment-requests/my"));

        // ===== Feed / Status / Info =====
        public async Task<FeedResponse> GetFeedAsync(DateTime? since = null)
        {
            var url = $"{BaseUrl}/api/feed";
            if (since.HasValue) url += $"?since={since.Value.ToUniversalTime():O}";
            return await Read<FeedResponse>(await _http.GetAsync(url));
        }

        public async Task<IReadOnlyList<SubjectStatusDto>> GetStatusPerSubjectAsync() =>
            await Read<List<SubjectStatusDto>>(await _http.GetAsync($"{BaseUrl}/api/status/per-subject"));

        public async Task<SystemInfoDto> GetSystemInfoAsync() =>
            await Read<SystemInfoDto>(await _http.GetAsync($"{BaseUrl}/api/system/info"));
        // =======================

        public async Task StartNotificationsAsync()
        {
            if (_hub != null) return;

            var hubUrl = $"{BaseUrl}/hubs/notify"; // BaseUrl já é https://escolainfosysapi.somee.com
            _hub = new HubConnectionBuilder()
                .WithUrl(hubUrl, opt =>
                {
                    if (!string.IsNullOrWhiteSpace(_token))
                        opt.AccessTokenProvider = () => Task.FromResult(_token)!;
                })
                .WithAutomaticReconnect()
                .Build();

            _hub.On<GradeNotification>("gradeAdded", g => GradePushed?.Invoke(this, g));
            _hub.On<StatusNotification>("statusChanged", s => StatusPushed?.Invoke(this, s));

            try { await _hub.StartAsync(); }
            catch { /* se falhar, cai no fallback de polling (B) */ }
        }


        // Iniciar polling do feed
        public void StartFeedPolling(TimeSpan? period = null, Action<IReadOnlyList<FeedItem>>? onNew = null)
        {
            StopFeedPolling(); // garante uma única instância

            _feedCts = new CancellationTokenSource();
            var every = period ?? TimeSpan.FromSeconds(30);

            _ = Task.Run(async () =>
            {
                while (!_feedCts!.IsCancellationRequested)
                {
                    try
                    {
                        var feed = await GetFeedAsync(_feedSince);
                        var list = feed.items?.ToList() ?? new List<FeedItem>();
                        if (list.Count > 0)
                        {
                            onNew?.Invoke(list);
                            var max = list.Max(i => i.Date);
                            if (max > _feedSince) _feedSince = max;
                        }
                    }
                    catch { /* ignora e tenta de novo */ }

                    try { await Task.Delay(every, _feedCts.Token); }
                    catch (TaskCanceledException) { }
                }
            });
        }

        // Parar polling (use ao deslogar)
        public void StopFeedPolling()
        {
            _feedCts?.Cancel();
            _feedCts = null;
        }

    }
}
