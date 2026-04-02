using SMARTrackMobile.Resources.Strings;
using SMARTrackMobile.Services;

namespace SMARTrackMobile.Views.Pages;

public partial class AboutPage : ContentPage
{
    private readonly SettingsService _settings;
    private readonly LocalizationService _loc;

    public AboutPage()
    {
        InitializeComponent();

        var services = Shell.Current?.Handler?.MauiContext?.Services
                       ?? throw new InvalidOperationException("Maui services not available.");

        _settings = services.GetService<SettingsService>()
                    ?? throw new InvalidOperationException("SettingsService not available.");
        _loc = services.GetService<LocalizationService>()
               ?? throw new InvalidOperationException("LocalizationService not available.");

        BindingContext = new AboutViewModel(_settings);

        ApplyStrings();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        ApplyStrings();
    }

    private void ApplyStrings()
    {
        Title = AppResources.About_Title;
        TitleLabel.Text = AppResources.About_Title;
        ShowSettingsLabel.Text = AppResources.About_LongPressToast;
        ScanQrLabel.Text = AppResources.About_LongPressQr;
    }

    private async void OnLanguageClicked(object sender, EventArgs e)
    {
        var choice = await DisplayActionSheet(
            AppResources.About_LangMenu,
            "Cancel",
            null,
            AppResources.Lang_English,
            AppResources.Lang_Spanish);

        if (choice == AppResources.Lang_English)
            _loc.SetLanguage("en");
        else if (choice == AppResources.Lang_Spanish)
            _loc.SetLanguage("es");
        else
            return;

        await DisplayAlert(AppResources.About_LangMenu, AppResources.Lang_Changed, "OK");

#if ANDROID
        Platform.CurrentActivity?.Recreate();
#endif
    }

    private sealed class AboutViewModel
    {
        private readonly SettingsService _settings;

        public Command ShowSettingsToastCommand { get; }
        public Command ScanQrForSettingsCommand { get; }
        public Command ScanDataMatrixCommand { get; }
        public Command PhotoUploadCommand { get; }

        public AboutViewModel(SettingsService settings)
        {
            _settings = settings;

            ShowSettingsToastCommand = new Command(() =>
            {
#if ANDROID
                ToastService.Show(_settings.DebugDump());
#endif
            });

            ScanQrForSettingsCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync("qr-scan");
            });

            ScanDataMatrixCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync("datamatrix-scan");
            });

            PhotoUploadCommand = new Command(async () =>
            {
                await Shell.Current.GoToAsync("photo-upload");
            });
        }
    }
}
