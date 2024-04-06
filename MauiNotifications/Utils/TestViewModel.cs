using System.Windows.Input;

namespace MauiNotifications.Utils
{
    internal class TestViewModel
    {
        public ICommand EditorCompleted { get; set; }

        public TestViewModel()
        {
            EditorCompleted = new Command(() =>
            {

            });
        }
    }
}
