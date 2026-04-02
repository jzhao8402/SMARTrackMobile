using System.Text.RegularExpressions;
using SMARTrackMobile.Services;

namespace SMARTrackMobile.Views.Pages;

public partial class DeltaSignInPage : ContentPage
{
    private readonly SettingsService _settings;
    private bool _completed;

    private static readonly string[] HtmlParseUrlHints =
    {
        "callback", "result", "complete", "success", "error"
    };

    private string _lastUrl = string.Empty;

    public DeltaSignInPage()
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

        AuthService.SetLoginType(LoginType.Delta);

#if ANDROID
        DeltaWebViewHtmlHub.HtmlReceived += OnHtmlReceived;
#endif

        var startUri = _settings.Load().DeltaSsoUri;
        LoadingOverlay.IsVisible = true;
        SsoWebView.Source = startUri;
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
#if ANDROID
        DeltaWebViewHtmlHub.HtmlReceived -= OnHtmlReceived;
#endif
    }

    private void OnNavigated(object sender, WebNavigatedEventArgs e)
    {
        LoadingOverlay.IsVisible = false;
        _lastUrl = e.Url ?? string.Empty;
    }

    private async void OnHtmlReceived(object? sender, string html)
    {
        if (_completed)
            return;

        if (!LooksLikeTerminalUrl(_lastUrl))
            return;

        var (success, code, error) = ParseHtml(html);

        if (success && !string.IsNullOrWhiteSpace(code))
        {
            _completed = true;
            AuthService.Login(LoginType.Delta);

            var services = Shell.Current?.Handler?.MauiContext?.Services;
            var enableSounds = services?.GetService<SettingsService>()?.Load().EnableSounds ?? true;

            if (enableSounds)
            {
                var sound = services?.GetService<SoundService>();
                if (sound is not null)
                    await sound.PlayLoginSuccessAsync();
            }

            await Shell.Current.GoToAsync("//dashboard");
        }
        else if (!success && !string.IsNullOrWhiteSpace(error))
        {
            _completed = true;
            await DisplayAlert("Delta Sign In failed", error, "OK");
        }
    }

    private static bool LooksLikeTerminalUrl(string url)
    {
        if (string.IsNullOrWhiteSpace(url))
            return false;

        return HtmlParseUrlHints.Any(h => url.Contains(h, StringComparison.OrdinalIgnoreCase));
    }

    // TODO: tune these patterns to your real HTML
    private static (bool IsSuccess, string? OneTimeCode, string? ErrorMessage) ParseHtml(string html)
    {
        var attrCode = Regex.Match(html, @"data-one-time-code\s*=\s*[""'](?<code>[^""']+)[""']",
            RegexOptions.IgnoreCase);
        if (attrCode.Success)
            return (true, attrCode.Groups["code"].Value, null);

        var textCode = Regex.Match(html, @"one[\s-]*time[\s-]*code\s*[:\-]\s*(?<code>[A-Za-z0-9]+)",
            RegexOptions.IgnoreCase);
        if (textCode.Success)
            return (true, textCode.Groups["code"].Value, null);

        var err = Regex.Match(html, @"class\s*=\s*[""'][^""']*(error|alert|validation)[^""']*[""'][^>]*>\s*(?<msg>[^<]{1,200})\s*<",
            RegexOptions.IgnoreCase);
        if (err.Success)
            return (false, null, err.Groups["msg"].Value.Trim());

        var err2 = Regex.Match(html, @"error\s*[:\-]\s*(?<msg>.{1,160})",
            RegexOptions.IgnoreCase);
        if (err2.Success)
            return (false, null, err2.Groups["msg"].Value.Trim());

        return (false, null, null);
    }
}
