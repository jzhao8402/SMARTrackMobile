using SMARTrackMobile.Services;

namespace SMARTrackMobile.Views.Pages;

public partial class PhotoUploadPage : ContentPage
{
    private readonly PhotoCaptureService _photo;
    private readonly BlobUploadService _upload;

    public PhotoUploadPage()
    {
        InitializeComponent();

        var services = Shell.Current?.Handler?.MauiContext?.Services
                       ?? throw new InvalidOperationException("Maui services not available.");

        _photo = services.GetService<PhotoCaptureService>()
                 ?? throw new InvalidOperationException("PhotoCaptureService not available.");
        _upload = services.GetService<BlobUploadService>()
                  ?? throw new InvalidOperationException("BlobUploadService not available.");
    }

    private async void OnTakePhotoClicked(object sender, EventArgs e)
    {
        Busy.IsVisible = Busy.IsRunning = true;
        ResultLabel.Text = "";

        try
        {
            var file = await _photo.CapturePhotoAsync();
            if (file is null)
            {
                ResultLabel.Text = "Capture not available or canceled.";
                return;
            }

            await using var stream = await file.OpenReadAsync();

            var contentType = "image/jpeg";
            var name = file.FileName ?? "photo.jpg";

            var (ok, msg, uri) = await _upload.UploadAsync(stream, name, contentType);

            ResultLabel.Text = ok ? $"{msg}\n{uri}" : msg;

#if ANDROID
            ToastService.Show(ok ? "Photo uploaded." : "Upload failed.");
#endif
        }
        finally
        {
            Busy.IsVisible = Busy.IsRunning = false;
        }
    }
}
