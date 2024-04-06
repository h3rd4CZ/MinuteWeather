namespace MauiNotifications.Pages;

public partial class Close : ContentPage
{
	public Close()
	{
		InitializeComponent();

		Application.Current.Quit();
	}
}