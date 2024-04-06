using System.Globalization;

namespace MauiNotifications.Utils
{
    class ThemeBindingToIconColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var color = (Color)value;

            return Application.Current.RequestedTheme == AppTheme.Dark
                ? new Color(255, 255, 255)
                : color;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}
