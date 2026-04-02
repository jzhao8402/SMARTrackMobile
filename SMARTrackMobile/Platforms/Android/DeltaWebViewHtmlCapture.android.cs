using Android.Webkit;
using Microsoft.Maui.Handlers;

namespace SMARTrackMobile;

public static class DeltaWebViewHtmlCapture
{
    public static void Enable()
    {
        WebViewHandler.Mapper.AppendToMapping("DeltaHtmlCapture", (handler, view) =>
        {
            if (handler.PlatformView is not Android.Webkit.WebView wv)
                return;

            wv.Settings.JavaScriptEnabled = true;
            wv.Settings.DomStorageEnabled = true;

            wv.SetWebViewClient(new HtmlCaptureWebViewClient(handler));
        });
    }

    private sealed class HtmlCaptureWebViewClient : WebViewClient
    {
        private readonly WebViewHandler _handler;

        public HtmlCaptureWebViewClient(WebViewHandler handler)
        {
            _handler = handler;
        }

        public override void OnPageFinished(Android.Webkit.WebView? view, string? url)
        {
            base.OnPageFinished(view, url);

            if (view is null)
                return;

            view.EvaluateJavascript(
                "(function(){return document.documentElement.outerHTML;})()",
                new HtmlValueCallback(_handler));
        }
    }

    private sealed class HtmlValueCallback : Java.Lang.Object, IValueCallback
    {
        private readonly WebViewHandler _handler;

        public HtmlValueCallback(WebViewHandler handler)
        {
            _handler = handler;
        }

        public void OnReceiveValue(Java.Lang.Object? value)
        {
            var jsonEncoded = value?.ToString() ?? string.Empty;
            var html = DecodeJsonString(jsonEncoded);
            DeltaWebViewHtmlHub.RaiseHtmlReceived(html);
        }

        private static string DecodeJsonString(string jsonEncoded)
        {
            try
            {
                return System.Text.Json.JsonSerializer.Deserialize<string>(jsonEncoded) ?? string.Empty;
            }
            catch
            {
                return jsonEncoded.Trim().Trim('"');
            }
        }
    }
}
