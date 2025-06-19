using SmartClipboard.Services;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace SmartClipboard.Utilities.Converters
{
    class ContentTypeConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value switch
            {
                ContentType.All => "💯",
                ContentType.Text => "📝",
                ContentType.Email => "📧",
                ContentType.Code => "{ }",
                ContentType.Image => "🖼",
                ContentType.File => "📁",
                ContentType.Link => "🌐",
                _ => "❓"
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
