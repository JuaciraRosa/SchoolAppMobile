using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AppSchoolMaui.Converters
{
    public sealed class FeedTypeToImageConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var t = (value as string ?? "").ToUpperInvariant();
            return t switch
            {
                "MARK" => "grade.png",
                "STATUS" => "info.png",
                "ABSENCE" => "info.png",
                _ => "info.png"
            };
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
