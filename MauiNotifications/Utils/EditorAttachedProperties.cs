using System.Windows.Input;

namespace MauiNotifications.Utils
{
    public class EditorAttached
    {
        public ICommand tmpCmd;

        public static readonly BindableProperty HasShadowProperty =
            BindableProperty.CreateAttached("HasShadow", typeof(bool),
                                   typeof(View), null, propertyChanged: completedCommandPropertyChanged);

        public static bool GetHasShadow(BindableObject bindable)
            => (bool)bindable.GetValue(HasShadowProperty);

        public static void SetCompletedCommand(BindableObject bindable, bool value)
            => bindable.SetValue(HasShadowProperty, value);

        private static void completedCommandPropertyChanged(BindableObject bindable, object oldValue, object newValue)
        {
            if (bindable is Button btn)
            {
                btn.Shadow = new Shadow { Brush = Colors.Violet, Radius = 15 };
            }
        }
    }
}
