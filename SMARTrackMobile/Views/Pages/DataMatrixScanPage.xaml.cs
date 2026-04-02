using ZXing.Net.Maui;

namespace SMARTrackMobile.Views.Pages;

public partial class DataMatrixScanPage : ContentPage
{
    private bool _handled;

    public DataMatrixScanPage()
    {
        InitializeComponent();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        CameraView.Options = new BarcodeReaderOptions
        {
            Formats = BarcodeFormat.DataMatrix,
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

#if ANDROID
        ToastService.Show($"Data Matrix: {value}");
#endif
        await DisplayAlert("Data Matrix", value, "OK");
        await Shell.Current.GoToAsync("..");
    }
}
