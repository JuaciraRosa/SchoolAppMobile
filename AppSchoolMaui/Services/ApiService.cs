using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Microsoft.Maui.Storage; 
using Microsoft.Maui.ApplicationModel; 
    



namespace AppSchoolMaui.Services
{
    public sealed class ApiService
    {
        // ==== Config ====
        public string BaseUrl { get; } = "https://escolainfosysapi.somee.com"; // API
        public string WebBase { get; } = "https://www.escolainfosys.somee.com"; // MVC (site)

        private readonly HttpClient _http;




        private readonly JsonSerializerOptions _json = new()
        {
            PropertyNameCaseInsensitive = true,
            ReadCommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true
        };




        public ApiService()
        {
            // Handler com descompressão p/ respostas gzip/deflate
            var handler = new HttpClientHandler
            {
                AutomaticDecompression = System.Net.DecompressionMethods.GZip | System.Net.DecompressionMethods.Deflate
            };

            _http = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };
            _http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            // Ajuda servidores/Firewalls chatos a identificar o cliente
            _http.DefaultRequestHeaders.UserAgent.ParseAdd("EscolaInfoSys.Maui/1.0");
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

        // ---- leitura mais robusta, com mensagem de erro do servidor ----
        private async Task<T> Read<T>(HttpResponseMessage resp)
        {
            var url = resp.RequestMessage?.RequestUri?.ToString() ?? "?";
            var txt = await resp.Content.ReadAsStringAsync();

            if (!resp.IsSuccessStatusCode)
            {
                string? serverMsg = null;
                try
                {
                    using var doc = JsonDocument.Parse(string.IsNullOrWhiteSpace(txt) ? "{}" : txt);
                    if (doc.RootElement.TryGetProperty("message", out var m)) serverMsg = m.GetString();
                    else if (doc.RootElement.TryGetProperty("error", out var e)) serverMsg = e.GetString();
                    else if (doc.RootElement.TryGetProperty("title", out var t)) serverMsg = t.GetString();
                }
                catch { /* ignore parse errors */ }

                var reason = $"{(int)resp.StatusCode} {resp.ReasonPhrase}";
                if (!string.IsNullOrWhiteSpace(serverMsg)) reason += $" - {serverMsg}";

                throw new HttpRequestException($"{reason} at {url}\n{txt}");
            }

            // quando o endpoint não tem corpo (ex.: 204), devolve default(T)
            if (typeof(T) == typeof(object) || string.IsNullOrWhiteSpace(txt))
                return default!;

            return JsonSerializer.Deserialize<T>(txt, _json)!;
        }

        // ========= Auth =========

        public record LoginReq(string Email, string Password);
        public record LoginResp(string Token, DateTime ExpiresAtUtc);

        public async Task LoginAsync(string email, string password, CancellationToken ct = default)
        {
            // normaliza e valida antes de enviar
            email = (email ?? string.Empty).Trim().ToLowerInvariant();
            password = password ?? string.Empty;

            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrEmpty(password))
                throw new HttpRequestException("Email e password são obrigatórios.");

            using var resp = await _http.PostAsync($"{BaseUrl}/api/auth/login",
                Body(new LoginReq(email, password)), ct);

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

        public async Task ForgotAsync(string email, CancellationToken ct = default)
        {
            using var resp = await _http.PostAsync($"{BaseUrl}/api/auth/forgot-password",
                Body(new ForgotPasswordReq((email ?? "").Trim())), ct);

            if (resp.IsSuccessStatusCode)
            {
                var txt = await resp.Content.ReadAsStringAsync(ct);
                if (!string.IsNullOrWhiteSpace(txt))
                {
                    var data = JsonSerializer.Deserialize<ForgotPasswordResp>(txt, _json);
                    if (!string.IsNullOrWhiteSpace(data?.link))
                        await Browser.OpenAsync(data.link, BrowserLaunchMode.External);
                }
            }
            else
            {
                await Read<object>(resp); // lança exceção detalhada
            }
        }

        // ApiService.cs
        public Task ForgotOnWebAsync(string? email = null)
        {
            var url = string.IsNullOrWhiteSpace(email)
                ? $"{WebBase}/Account/ForgotPassword"
                : $"{WebBase}/Account/ForgotPassword?email={Uri.EscapeDataString(email.Trim())}";
            return OpenUrlSafeAsync(url);
        }

        private static async Task OpenUrlSafeAsync(string url)
        {
            try { await Browser.OpenAsync(new Uri(url), BrowserLaunchMode.External); }
            catch (Microsoft.Maui.ApplicationModel.FeatureNotSupportedException)
            { await Launcher.OpenAsync(url); }
#if WINDOWS
            catch
            {
                try { System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo(url) { UseShellExecute = true }); }
                catch { }
            }
#endif
        }
        public bool HasAuth =>
       _http.DefaultRequestHeaders.Authorization is { Parameter.Length: > 0 };

        // ApiService.cs
        public Task ChangePasswordOnWebAsync()
       => OpenUrlSafeAsync("https://www.escolainfosys.somee.com/Account/ChangePassword");

        // ==== Change password (requer login / Bearer) ====
        // ==== Change password (requer login / Bearer) ====
        public record ChangePasswordReq(string CurrentPassword, string NewPassword, string ConfirmPassword);

        public async Task ChangePasswordAsync(string currentPassword, string newPassword, string confirmPassword, CancellationToken ct = default)
        {
            using var resp = await _http.PostAsync(
                $"{BaseUrl}/api/auth/change-password",
                Body(new ChangePasswordReq(currentPassword, newPassword, confirmPassword)), ct);

            // a API costuma devolver 200 OK sem corpo; se der erro, lança com o detalhe vindo do servidor
            if (!resp.IsSuccessStatusCode)
                await Read<object>(resp);
        }
        // === Reset password (sem estar autenticado) ===
        public record ResetPasswordReq(string Email, string Token, string NewPassword, string ConfirmPassword);

        public async Task ResetPasswordAsync(string email, string token, string newPassword, string confirmPassword, CancellationToken ct = default)
        {
            using var resp = await _http.PostAsync(
                $"{BaseUrl}/api/auth/reset-password",
                Body(new ResetPasswordReq(email, token, newPassword, confirmPassword)), ct);

            if (!resp.IsSuccessStatusCode)
                await Read<object>(resp); // lança com detalhes do servidor (400 validação, etc.)
        }



        // ========= Profile =========

        public record MeDto(string Email, string FullName, string Role, string? ProfilePhoto);

        public async Task<MeDto> GetProfileAsync(CancellationToken ct = default)
        {
            using var resp = await _http.GetAsync($"{BaseUrl}/api/students/profile", ct);
            return await Read<MeDto>(resp);
        }

        // Atualiza nome e/ou URL da foto
        public record UpdateProfileReq(string? FullName, string? ProfilePhoto);

        public async Task<MeDto> UpdateProfileAsync(string? fullName, string? profilePhoto, CancellationToken ct = default)
        {
            using var resp = await _http.PutAsync($"{BaseUrl}/api/students/profile",
                Body(new UpdateProfileReq(fullName, profilePhoto)), ct);

            if (resp.StatusCode == HttpStatusCode.NoContent)
                return await GetProfileAsync(ct);

            return await Read<MeDto>(resp);
        }

        // Upload foto
        public record UploadPhotoResp(string path, string url);

        public async Task<UploadPhotoResp> UploadStudentProfilePhotoAsync(byte[] bytes, string fileName, CancellationToken ct = default)
        {
            using var form = new MultipartFormDataContent();
            var file = new ByteArrayContent(bytes);
            file.Headers.ContentType = new MediaTypeHeaderValue(GetMime(fileName));
            form.Add(file, "file", fileName);

            using var resp = await _http.PostAsync($"{BaseUrl}/api/students/profile/photo", form, ct);
            return await Read<UploadPhotoResp>(resp);
        }

        // Converte string (fname / caminho relativo / url absoluta) em URI absolute
        public Task<Uri?> ResolvePhotoUriAsync(string? pathOrUrl)
        {
            if (string.IsNullOrWhiteSpace(pathOrUrl))
                return Task.FromResult<Uri?>(null);

            if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var abs))
                return Task.FromResult<Uri?>(abs);

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

        public async Task<List<CourseDto>> GetPublicCoursesAsync(CancellationToken ct = default)
        {
            using var resp = await _http.GetAsync($"{BaseUrl}/api/public/courses", ct);
            return await Read<List<CourseDto>>(resp);
        }

        public async Task<List<SubjectDto>> GetPublicSubjectsAsync(int? courseId = null, CancellationToken ct = default)
        {
            var url = $"{BaseUrl}/api/public/subjects";
            if (courseId is not null) url += $"?courseId={courseId.Value}";
            using var resp = await _http.GetAsync(url, ct);
            return await Read<List<SubjectDto>>(resp);
        }

        // ========= Notas =========

        public record MarkDto(int Id, int SubjectId, string Subject, string EvaluationType,
                              float Value, bool? IsPassed, DateTime Date);

        public async Task<List<MarkDto>> GetMarksAsync(int? subjectId = null, CancellationToken ct = default)
        {
            var url = $"{BaseUrl}/api/marks";
            if (subjectId is not null) url += $"?subjectId={subjectId.Value}";
            using var resp = await _http.GetAsync(url, ct);
            return await Read<List<MarkDto>>(resp);
        }

        public async Task<Dictionary<int, double>> GetMyAveragesAsync(CancellationToken ct = default)
        {
            using var resp = await _http.GetAsync($"{BaseUrl}/api/marks/averages", ct);
            return await Read<Dictionary<int, double>>(resp);
        }

        // ========= Faltas =========

        public record AbsenceItemDto(int Id, int SubjectId, string? Subject, DateTime Date, bool Justified);
        public record AbsencePerSubjectDto(int SubjectId, string? Subject, int Count, double Percentage,
                                           int MaxAllowed, bool Exceeded, bool IsExcluded);
        public record AbsenceOverallDto(bool AnyExcluded, bool AnyExceeded);
        public record AbsenceResponse(List<AbsenceItemDto> items, AbsenceSummary summary);
        public record AbsenceSummary(List<AbsencePerSubjectDto> perSubject, AbsenceOverallDto overall);

        public async Task<AbsenceResponse> GetAbsencesAsync(CancellationToken ct = default)
        {
            using var resp = await _http.GetAsync($"{BaseUrl}/api/absences", ct);
            return await Read<AbsenceResponse>(resp);
        }

        // ========= Status =========

        public record StatusDto(int SubjectId, string Subject, double? Average, double? PassThreshold,
                                int Absences, int MaxAbsences, bool ExceededAbsences, string Status);

        public async Task<List<StatusDto>> GetStatusAsync(CancellationToken ct = default)
        {
            using var resp = await _http.GetAsync($"{BaseUrl}/api/status/per-subject", ct);
            return await Read<List<StatusDto>>(resp);
        }

        // ========= Feed =========

        public record FeedItem(string Type, int Id, int SubjectId, string? Subject,
                               DateTime Date, float? Value, string? EvaluationType, bool? IsPassed);
        public record FeedResponse(DateTime since, int count, List<FeedItem> items);

        public async Task<FeedResponse> GetFeedAsync(DateTime? sinceUtc = null, CancellationToken ct = default)
        {
            var url = $"{BaseUrl}/api/feed";
            if (sinceUtc is not null)
                url += $"?since={Uri.EscapeDataString(sinceUtc.Value.ToUniversalTime().ToString("o"))}";

            using var resp = await _http.GetAsync(url, ct);
            return await Read<FeedResponse>(resp);
        }

        // polling simples para avisos (usado após login)
        private System.Timers.Timer? _timer;
        private DateTime? _lastSince;

      
        private readonly HashSet<int> _notifiedIds = new();      // evita alertas repetidos na sessão
        private const string LastFeedSeenKey = "last_feed_seen_utc"; // chave no Preferences
        private DateTime _lastSeenUtc = DateTime.MinValue;           // último item visto (UTC)
        private DateTime _burstUntilUtc;

        public void StartFeedPolling(TimeSpan interval, Func<List<FeedItem>, Task> onItems, TimeSpan? initialBurst = null)
        {
            _timer?.Stop();
            _timer?.Dispose();

            // carrega último visto (mantém tua lógica atual)
            var saved = Preferences.Get(LastFeedSeenKey, string.Empty);
            if (DateTime.TryParse(saved, null, System.Globalization.DateTimeStyles.RoundtripKind, out var d))
            {
                _lastSeenUtc = d.Kind == DateTimeKind.Utc ? d : d.ToUniversalTime();
                var now = DateTime.UtcNow;
                if (_lastSeenUtc > now) _lastSeenUtc = now; // clamp se relógio mudou
            }
            else
            {
                _lastSeenUtc = DateTime.MinValue;
            }

            // since inicial
            _lastSince = _lastSeenUtc == DateTime.MinValue
                ? DateTime.UtcNow.AddDays(-7)
                : _lastSeenUtc.AddSeconds(-1);

            // burst: 2s por 30s (ou o valor que passar)
            var burst = initialBurst ?? TimeSpan.FromSeconds(30);
            _burstUntilUtc = DateTime.UtcNow + burst;

            _timer = new System.Timers.Timer
            {
                AutoReset = true,
                Interval = burst > TimeSpan.Zero ? 2000 : interval.TotalMilliseconds
            };

            _timer.Elapsed += async (_, __) =>
            {
                // quando acabar o burst, volta pro intervalo normal
                if (DateTime.UtcNow > _burstUntilUtc && Math.Abs(_timer.Interval - interval.TotalMilliseconds) > 0.1)
                    _timer.Interval = interval.TotalMilliseconds;

                await FeedTickAsync(onItems);
            };

            _timer.Start();

            // >>> leading-edge: dispara já, sem esperar o primeiro intervalo
            _ = FeedTickAsync(onItems);
        }

        private async Task FeedTickAsync(Func<List<FeedItem>, Task> onItems)
        {
            try
            {
                var data = await GetFeedAsync(_lastSince);

                var fresh = data.items
                    .Where(i => i.Date.ToUniversalTime() > _lastSeenUtc && _notifiedIds.Add(i.Id))
                    .OrderBy(i => i.Date)
                    .ToList();

                if (fresh.Count > 0)
                {
                    await onItems(fresh);

                    _lastSeenUtc = fresh.Max(i => i.Date.ToUniversalTime());
                    Preferences.Set(LastFeedSeenKey, _lastSeenUtc.ToString("o"));

                    // “since” levemente antes para não perder a borda inclusiva
                    _lastSince = _lastSeenUtc.AddSeconds(-1);
                }
            }
            catch
            {
                // silencia erros de polling
            }
        }

        public Task ForceFeedCheckAsync(Func<List<FeedItem>, Task> onItems)
    => FeedTickAsync(onItems);



        public void StopFeedPolling()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
            _lastSince = null;
            _notifiedIds.Clear(); /*  lembra o último visto*/
        }

        public void ResetFeedSeen()
        {
            Preferences.Remove(LastFeedSeenKey);
            _lastSeenUtc = DateTime.MinValue;
            _notifiedIds.Clear();
        }




        // ========= Enrollment Requests =========
        public record EnrollmentRequestDto(int Id, int StudentId, int SubjectId, string? Subject,
                                           string? Status, DateTime? CreatedAt);
        public record CreateEnrollmentReq(int SubjectId, string? Note);

        public async Task<List<EnrollmentRequestDto>> GetMyEnrollmentRequestsAsync(CancellationToken ct = default)
        {
            using var resp = await _http.GetAsync($"{BaseUrl}/api/EnrollmentRequests/my", ct);
            return await Read<List<EnrollmentRequestDto>>(resp);
        }

        public async Task CreateEnrollmentRequestAsync(int subjectId, string? note, CancellationToken ct = default)
        {
            using var resp = await _http.PostAsync(
                $"{BaseUrl}/api/EnrollmentRequests",
                Body(new CreateEnrollmentReq(subjectId, note)),
                ct);
            await Read<object>(resp); // valida/lança erro se houver
        }





    }

}
