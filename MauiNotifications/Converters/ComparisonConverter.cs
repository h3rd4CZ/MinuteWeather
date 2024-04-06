using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MauiNotifications.Converters
{
    public class ComparisonConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var fmt = new NumberFormatInfo();
            fmt.NegativeSign = "-";

            var allParams = ((string)parameter).Split((';'));
            var compValue = Double.Parse(allParams[0], fmt);
            var comparison = allParams[1];

            switch (comparison)
            {
                case "<":
                    return ((double)value) < compValue;
                case ">":
                    return ((double)value) > compValue;
                case "!=":
                    return ((double)value) != compValue;
                case "==":
                default:
                    return ((double)value) == compValue;
            }
        }
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}
