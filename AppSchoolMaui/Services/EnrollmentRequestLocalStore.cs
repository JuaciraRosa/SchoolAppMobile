using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AppSchoolMaui.Services
{
    public static class EnrollmentRequestLocalStore
    {
        static readonly string FilePath = Path.Combine(FileSystem.AppDataDirectory, "enroll-requests.json");
        static readonly JsonSerializerOptions Json = new() { WriteIndented = true };

        public static async Task<List<ApiService.EnrollmentRequestDto>> LoadAsync()
        {
            try
            {
                if (!File.Exists(FilePath)) return new();
                var txt = await File.ReadAllTextAsync(FilePath);
                return JsonSerializer.Deserialize<List<ApiService.EnrollmentRequestDto>>(txt) ?? new();
            }
            catch { return new(); }
        }

        static async Task SaveAsync(List<ApiService.EnrollmentRequestDto> list)
        {
            var txt = JsonSerializer.Serialize(list, Json);
            Directory.CreateDirectory(Path.GetDirectoryName(FilePath)!);
            await File.WriteAllTextAsync(FilePath, txt);
        }

        public static async Task AddAsync(int subjectId, string? subject, string? note)
        {
            var all = await LoadAsync();
            var nextId = (all.Count == 0 ? 1 : all.Max(x => x.Id) + 1);

            // StudentId não interessa no modo local → 0
            all.Add(new ApiService.EnrollmentRequestDto(
                Id: nextId,
                StudentId: 0,
                SubjectId: subjectId,
                Subject: string.IsNullOrWhiteSpace(subject) ? $"Subject {subjectId}" : subject,
                Status: "Pending (local)",
                CreatedAt: DateTime.UtcNow
            ));

            await SaveAsync(all);
        }
    }
}
