using MauiNotifications.Services;

namespace MauiNotifications.Pages;

public partial class Debug : ContentPage
{
    private readonly IMessage message;
    private readonly UserFileDataService userFileDataService;

    public Debug(IMessage message, UserFileDataService userFileDataService)
    {
        InitializeComponent();
        this.message = message;
        this.userFileDataService = userFileDataService;
    }

    protected override async void OnAppearing()
    {
        try
        {
            await LoadLog();
        }
        catch (Exception ex)
        {
            message.ShowMessage($"ERR : {ex}");
        }
    }

    private async Task LoadLog()
    {
        var data = await userFileDataService.ReadAllData();

        var items = new List<LogItem>();

        foreach (var item in data)
        {
            var logItemData = item.Split(new[] { UserFileDataService.LOG_DELIMITER }, StringSplitOptions.RemoveEmptyEntries);

            if (logItemData.Count() == 2)
            {
                DateTime.TryParse(logItemData[0], out DateTime dtm);

                items.Add(new LogItem(dtm, logItemData[1]));
            }
        }

        items = items.OrderByDescending(i => i.date).ToList();

        log.ItemsSource = items;
    }

    private async void DeleteLog(object sender, EventArgs e)
    {
        userFileDataService.DeleteAllData();

        await LoadLog();
    }
}