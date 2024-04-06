using MauiNotifications.Model.MVVM;
using System.Globalization;

namespace MauiNotifications.Converters
{
    public class GeofenceCurrentPlaceToObjectConverter : GeofenceCurrentPlaceToObjectConverter<object> { }
    
    public class GeofenceCurrentPlaceToObjectConverter<TObject> : IValueConverter
    {
        public TObject TrueObject { get; set; }
        public TObject FalseObject { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is uint geofencePlaceId)) throw new InvalidOperationException("Value must be uint id");
            
            if(parameter is ContentPage contentPage)
            {
                if(contentPage.BindingContext is GeofencePreferenceModel vm)
                {
                    return vm.CurrentPlace == geofencePlaceId ? Process(TrueObject) : Process(FalseObject);
                }
                else throw new InvalidOperationException($"Binding context must be of type : {nameof(GeofencePreferenceModel)}");
            }
            else throw new InvalidOperationException($"Converter parameter must be of type : {nameof(contentPage)}");
        }

        private object Process(TObject @object)
        {
            if(@object is Binding binding)
            {
                IMarkupExtension be = new BindingExtension()
                {
                    Path = binding.Path,
                    Source = binding.Source,
                    Converter = binding.Converter,
                    ConverterParameter = binding.ConverterParameter,
                    Mode = binding.Mode,
                    StringFormat = binding.StringFormat
                };

                return be.ProvideValue(MauiProgram.Services);
            }
            
            return @object;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return default;
        }
    }
}
