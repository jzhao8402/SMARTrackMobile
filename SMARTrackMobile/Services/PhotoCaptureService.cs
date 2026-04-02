namespace SMARTrackMobile.Services;

public class PhotoCaptureService
{
    public async Task<FileResult?> CapturePhotoAsync()
    {
        try
        {
            if (!MediaPicker.Default.IsCaptureSupported)
                return null;

            return await MediaPicker.Default.CapturePhotoAsync();
        }
        catch
        {
            return null;
        }
    }
}
