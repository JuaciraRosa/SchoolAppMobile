using SchoolAppMobile.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SchoolAppMobile.Services
{
    public interface IApiService
    {
        Task<T?> GetAsync<T>(string endpoint);
        Task<List<T>> GetListAsync<T>(string endpoint);
        Task<bool> PostAsync<T>(string endpoint, T data);
        Task<string?> GetToken(); // para acesso ao token se necessário
        Task<bool> PostRawAsync(string endpoint, string rawString);
        Task<string?> LoginAsync(LoginDto loginDto);

        Task<bool> UpdateStudentProfileAsync(string newUsername, FileResult? photoFile);

        Task<bool> ForgotPasswordAsync(string email);

        Task<bool> ResetPasswordAsync(string email, string token, string newPassword);

        Task<bool> ChangePasswordAsync(ChangePasswordDto dto);

        // ---- públicos (ANÓNIMO)
        Task<List<CourseDto>> GetPublicCoursesAsync();
        Task<List<SubjectDto>> GetPublicSubjectsAsync();
        Task<List<FormGroupDto>> GetPublicFormGroupsAsync();

     
        Task<List<SubjectDto>> GetPublicSubjectsByCourseAsync(int courseId);
        Task<List<ContentDto>> GetPublicContentsBySubjectAsync(int subjectId);



    }
}
