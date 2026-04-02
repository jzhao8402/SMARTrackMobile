namespace SMARTrackMobile;

public static class DeltaWebViewHtmlHub
{
    public static event EventHandler<string>? HtmlReceived;

    public static void RaiseHtmlReceived(string html)
        => HtmlReceived?.Invoke(null, html);
}
