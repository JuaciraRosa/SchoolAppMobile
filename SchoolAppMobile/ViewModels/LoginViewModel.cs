using SchoolAppMobile.Models;
using SchoolAppMobile.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SchoolAppMobile.ViewModels
{
    public class LoginViewModel : BindableObject
    {
        private readonly ApiService _apiService;

        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public ICommand LoginCommand { get; }

        public LoginViewModel()
        {
            _apiService = new ApiService();
            LoginCommand = new Command(async () => await LoginAsync());
        }

        private async Task LoginAsync()
        {
            try
            {
                var loginRequest = new LoginDto { Email = Email, Password = Password };

                var token = await _apiService.LoginAsync(loginRequest);

                if (!string.IsNullOrEmpty(token))
                {
                    await Shell.Current.GoToAsync("//HomePage"); // rota que existe no teu AppShell
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Erro", "Login inválido", "OK");
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Erro", ex.Message, "OK");
            }
        }
    }

}



