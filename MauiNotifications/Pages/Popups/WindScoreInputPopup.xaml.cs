using CommunityToolkit.Maui.Views;

namespace MauiNotifications.Pages.Popups;

public partial class WindScoreInputPopup : Popup
{
	public WindScoreInputPopup()
	{
        InitializeComponent();
    }

    private void Button_Ok(object sender, EventArgs e)
    {
        this.Close((int)interval.Value);
    }

    private void Button_Cancel(object sender, EventArgs e)
    {
        this.Close();
    }
}