using MauiNotifications.Model.WeatherDaily;

namespace MauiNotifications.Template
{
    public class HotTopicDataTemplateSelector : DataTemplateSelector
    {
        public DataTemplate Alert { get; set; }
        public DataTemplate CurrentInfo { get; set; }

        protected override DataTemplate OnSelectTemplate(object item, BindableObject container)
        {
            HotTopicMessage htm = item as HotTopicMessage;

            return htm.Alert is not null ? Alert : CurrentInfo;
        }
    }
}
