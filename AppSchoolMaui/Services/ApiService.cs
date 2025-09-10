using Microsoft.Maui.ApplicationModel; 
using Microsoft.Maui.Storage; 
using System;
using System.Collections.Generic;
using System.Globalization;
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

        private const string DefaultAvatarPath = "/uploads/default.png";

        public Task<Uri?> ResolvePhotoUriAsync(string? pathOrUrl)
        {
            // 1) vazio => padrão
            if (string.IsNullOrWhiteSpace(pathOrUrl))
                return Task.FromResult<Uri?>(new Uri($"{WebBase}{DefaultAvatarPath}"));

            // 2) já é absoluta
            if (Uri.TryCreate(pathOrUrl, UriKind.Absolute, out var abs))
                return Task.FromResult<Uri?>(abs);

            // 3) normaliza relativos (ficheiro, uploads/..., wwwroot/uploads/...)
            var p = pathOrUrl.Trim().Replace('\\', '/').TrimStart('/');
            if (p.StartsWith("wwwroot/", StringComparison.OrdinalIgnoreCase))
                p = p.Substring("wwwroot/".Length);

            var i = p.IndexOf("uploads/", StringComparison.OrdinalIgnoreCase);
            if (i >= 0) p = p.Substring(i + "uploads/".Length);

            var url = $"{WebBase}/uploads/{p}";
            return Task.FromResult<Uri?>(new Uri(url));
        }


        public static string NormalizeUploadPath(string? urlOrPath)
        {
            if (string.IsNullOrWhiteSpace(urlOrPath)) return string.Empty;

            // Se for URL absoluta, extrai o caminho
            if (Uri.TryCreate(urlOrPath, UriKind.Absolute, out var abs))
            {
                var path = abs.AbsolutePath; // ex.: /uploads/abc.jpg
                return path.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase)
                    ? path
                    : path; // mantém o que vier do servidor
            }

            var p = urlOrPath.Trim();
            if (!p.StartsWith("/")) p = "/" + p;
            if (!p.StartsWith("/uploads/", StringComparison.OrdinalIgnoreCase))
                p = "/uploads/" + p.TrimStart('/');

            return p;
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
        #region Feed polling & change-detection

        // --- Persistência de IDs vistos (evita repetir e também permite aceitar data antiga) ---
        private const string SeenIdsKey = "feed_seen_ids_csv";
        private const int SeenIdsCap = 200;                     // guarda só os últimos 200
        private readonly LinkedList<int> _seenIds = new();
        private readonly HashSet<int> _seenIdsSet = new();

        private void LoadSeenIds()
        {
            _seenIds.Clear(); _seenIdsSet.Clear();
            var csv = Preferences.Get(SeenIdsKey, "");
            if (string.IsNullOrWhiteSpace(csv)) return;

            foreach (var p in csv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
                if (int.TryParse(p, out var id) && _seenIdsSet.Add(id))
                    _seenIds.AddLast(id);
        }

        private void SaveSeenIds()
        {
            while (_seenIds.Count > SeenIdsCap)
            {
                var first = _seenIds.First!.Value;
                _seenIds.RemoveFirst();
                _seenIdsSet.Remove(first);
            }
            Preferences.Set(SeenIdsKey, string.Join(",", _seenIds));
        }

        // --- Persistência de "estado" por item (para detectar UPDATE do conteúdo) ---
        private const string FeedDigestsKey = "feed_item_digests_json";
        private readonly Dictionary<int, string> _digests = new();

        private void LoadDigests()
        {
            _digests.Clear();
            var json = Preferences.Get(FeedDigestsKey, "");
            if (string.IsNullOrWhiteSpace(json)) return;
            try
            {
                var fromPref = JsonSerializer.Deserialize<Dictionary<int, string>>(json);
                if (fromPref is null) return;
                foreach (var kv in fromPref) _digests[kv.Key] = kv.Value;
            }
            catch { /* ignora corrupções/versão antiga */ }
        }

        private void SaveDigests()
        {
            try
            {
                // limita tamanho aproximando dos IDs mantidos
                if (_digests.Count > SeenIdsCap)
                {
                    foreach (var id in _digests.Keys.Except(_seenIdsSet).Take(_digests.Count - SeenIdsCap).ToList())
                        _digests.Remove(id);
                }
                Preferences.Set(FeedDigestsKey, JsonSerializer.Serialize(_digests));
            }
            catch { }
        }

        // fingerprint do conteúdo da nota (valor/aprovado/tipo/data)
        private static string MakeDigest(FeedItem i)
        {
            var val = i.Value?.ToString("0.###", CultureInfo.InvariantCulture) ?? "";
            var pass = i.IsPassed?.ToString() ?? "";
            var eval = i.EvaluationType ?? "";
            var date = i.Date.ToUniversalTime().ToString("o");
            return $"{val}|{pass}|{eval}|{date}";
        }

        // DTOs do feed
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

        private System.Timers.Timer? _timer;
        private DateTime? _lastSince;

        private readonly HashSet<int> _notifiedIds = new();      // evita repetição na sessão
        private const string LastFeedSeenKey = "last_feed_seen_utc";
        private DateTime _lastSeenUtc = DateTime.MinValue;
        private DateTime _burstUntilUtc;

        // >>> Evento opcional para páginas interessadas (ex.: MarksPage)
        public event EventHandler<List<FeedItem>>? FeedArrived;

        public void StartFeedPolling(TimeSpan interval, Func<List<FeedItem>, Task> onItems, TimeSpan? initialBurst = null)
        {
            _timer?.Stop();
            _timer?.Dispose();

            // ponteiro temporal
            var saved = Preferences.Get(LastFeedSeenKey, string.Empty);
            _lastSeenUtc = DateTime.TryParse(saved, null, DateTimeStyles.RoundtripKind, out var d)
                ? (d.Kind == DateTimeKind.Utc ? d : d.ToUniversalTime())
                : DateTime.MinValue;

            _lastSince = _lastSeenUtc == DateTime.MinValue
                ? DateTime.UtcNow.AddDays(-7)
                : _lastSeenUtc.AddMilliseconds(-500); // pequena margem

            // histórico
            LoadSeenIds();
            LoadDigests();

            _notifiedIds.Clear();
            var burst = initialBurst ?? TimeSpan.FromSeconds(30);
            _burstUntilUtc = DateTime.UtcNow + burst;

            _timer = new System.Timers.Timer
            {
                AutoReset = true,
                Interval = burst > TimeSpan.Zero ? 2000 : interval.TotalMilliseconds
            };

            _timer.Elapsed += async (_, __) =>
            {
                if (DateTime.UtcNow > _burstUntilUtc && Math.Abs(_timer.Interval - interval.TotalMilliseconds) > 0.1)
                    _timer.Interval = interval.TotalMilliseconds;

                await FeedTickAsync(onItems);
            };

            _timer.Start();

            // leading-edge: checa já
            _ = FeedTickAsync(onItems);
        }

        private async Task FeedTickAsync(Func<List<FeedItem>, Task> onItems)
        {
            try
            {
                var data = await GetFeedAsync(_lastSince);
                var incoming = (data.items ?? new()).OrderBy(i => i.Date).ToList();
                if (incoming.Count == 0) return;

                var fresh = new List<FeedItem>();
                foreach (var i in incoming)
                {
                    var utc = i.Date.ToUniversalTime();
                    var isNewById = !_seenIdsSet.Contains(i.Id) && _notifiedIds.Add(i.Id);
                    var isNewByDate = utc > _lastSeenUtc;

                    // detecta mudança de conteúdo mesmo mantendo a mesma data/Id
                    var digest = MakeDigest(i);
                    var isChanged = !_digests.TryGetValue(i.Id, out var old) || !string.Equals(old, digest, StringComparison.Ordinal);

                    if (isNewById || isNewByDate || isChanged)
                        fresh.Add(i);
                }

                // 1) snapshot: atualiza digests e “vistos” com TUDO que chegou
                foreach (var item in incoming)
                {
                    _digests[item.Id] = MakeDigest(item);
                    if (_seenIdsSet.Add(item.Id)) _seenIds.AddLast(item.Id);
                }
                SaveDigests();
                SaveSeenIds();

                // 2) avança ponteiro temporal
                var maxFresh = fresh.Max(i => i.Date.ToUniversalTime());
                if (maxFresh > _lastSeenUtc)
                {
                    _lastSeenUtc = maxFresh;
                    Preferences.Set(LastFeedSeenKey, _lastSeenUtc.ToString("o"));
                    _lastSince = _lastSeenUtc.AddSeconds(-1);
                }

                // 3) só agora dispara os listeners/callback (uma única vez)
                try { FeedArrived?.Invoke(this, fresh); } catch { }
                await onItems(fresh);

            }
            catch
            {
                // silencia erros de polling
            }
        }

        // checagem imediata (sem esperar o timer)
        public Task ForceFeedCheckAsync() => FeedTickAsync(_ => Task.CompletedTask);

        public void StopFeedPolling()
        {
            _timer?.Stop();
            _timer?.Dispose();
            _timer = null;
            _lastSince = null;
            _notifiedIds.Clear();
        }

        public void ResetFeedSeen()
        {
            Preferences.Remove(LastFeedSeenKey);
            Preferences.Remove(SeenIdsKey);
            Preferences.Remove(FeedDigestsKey);
            _lastSeenUtc = DateTime.MinValue;

            _seenIds.Clear(); _seenIdsSet.Clear();
            _digests.Clear();
            _notifiedIds.Clear();
        }

        #endregion
        // ======== FIM Feed =========



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
