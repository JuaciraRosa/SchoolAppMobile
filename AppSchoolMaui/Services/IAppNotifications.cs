using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSchoolMaui.Services
{
    public interface IAppNotifications
    {
        Task ShowAsync(string title, string message);
        Task RequestPermissionsAsync();     // pedir permissões (Android 13+/iOS)
       
    }
}
