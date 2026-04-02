using SMARTrackMobile.Services;
using ZXing.Net.Maui;

namespace SMARTrackMobile.Views.Pages;

public partial class QrScanPage : ContentPage
{
    private readonly SettingsService _settings;
    private bool _handled;

    public QrScanPage()
    {
        InitializeComponent();

        var services = Shell.Current?.Handler?.MauiContext?.Services
                       ?? throw new InvalidOperationException("Maui services not available.");

        _settings = services.GetService<SettingsService>()
                    ?? throw new InvalidOperationException("SettingsService not available.");
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        CameraView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.QrCode,
            AutoRotate = true,
            Multiple = false
        };
    }

    private async void OnBarcodesDetected(object sender, BarcodeDetectionEventArgs e)
    {
        if (_handled) return;

        var value = e.Results?.FirstOrDefault()?.Value;
        if (string.IsNullOrWhiteSpace(value))
            return;

        _handled = true;

        var ok = _settings.TryApplyFromQrPayload(value, out var message);

#if ANDROID
        ToastService.Show(message);
#endif

        await DisplayAlert(ok ? "Settings updated" : "QR parse failed", message, "OK");
        await Shell.Current.GoToAsync("..");
    }
}
