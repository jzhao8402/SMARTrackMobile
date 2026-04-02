using SMARTrackMobile.Services;

namespace SMARTrackMobile;

public class AppCloser : IAppCloser
{
    public void CloseApp()
    {
        var activity = Platform.CurrentActivity;
        activity?.FinishAffinity();
    }
}
