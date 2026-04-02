using Android.Widget;

namespace SMARTrackMobile;

public static class ToastService
{
    public static void Show(string message)
    {
        var activity = Platform.CurrentActivity;
        if (activity is null) return;

        Toast.MakeText(activity, message, ToastLength.Long)?.Show();
    }
}
