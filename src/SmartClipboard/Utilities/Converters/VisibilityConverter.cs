using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace SmartClipboard.Utilities.Converters
{
    class VisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool isVisible = true;

            if (value == null)
            {
                isVisible = false;
            }
            else if (value is string str)
            {
                isVisible = !string.IsNullOrWhiteSpace(str);
            }
            else if (value is System.Collections.IEnumerable enumerable && !(value is string))
            {
                isVisible = enumerable.GetEnumerator().MoveNext();
            }

            return isVisible ? Visibility.Collapsed : Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
