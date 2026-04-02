using Microsoft.Maui.Handlers;
using Plugin.Maui.Audio;
using SMARTrackMobile.Services;
using ZXing.Net.Maui.Controls;

namespace SMARTrackMobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();

        builder
            .UseMauiApp<App>()
            .UseBarcodeReader();

        // Services
        builder.Services.AddSingleton(AudioManager.Current);
        builder.Services.AddSingleton<SoundService>();
        builder.Services.AddSingleton<SettingsService>();
        builder.Services.AddSingleton<LocalizationService>();

        builder.Services.AddSingleton<BlobUploadService>();
        builder.Services.AddSingleton<PhotoCaptureService>();

        builder.Services.AddSingleton<ServerTimeService>();
        builder.Services.AddSingleton<ServiceAlarmScheduler>();

#if ANDROID
        builder.Services.AddSingleton<IAppCloser, AppCloser>();

        // Enable JavaScript + DOM storage for WebView
        WebViewHandler.Mapper.AppendToMapping("EnableJavascript", (handler, view) =>
        {
            if (handler.PlatformView is Android.Webkit.WebView wv)
            {
                wv.Settings.JavaScriptEnabled = true;
                wv.Settings.DomStorageEnabled = true;
                wv.Settings.JavaScriptCanOpenWindowsAutomatically = true;
                wv.Settings.SetSupportMultipleWindows(false);
            }
        });

        // Enable HTML capture hook for WebView
        DeltaWebViewHtmlCapture.Enable();
#endif

        return builder.Build();
    }
}
