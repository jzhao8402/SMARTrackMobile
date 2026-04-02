using SMARTrackMobile.Services;
using SMARTrackMobile.Views.Pages;

namespace SMARTrackMobile;

public partial class AppShell : Shell
{
    private static readonly HashSet<string> AuthRoutes = new(StringComparer.OrdinalIgnoreCase)
    {
        "dashboard", "bulletins", "profile", "service"
    };

    public AppShell()
    {
        InitializeComponent();

        // Apply saved language early
        var loc = Current?.Handler?.MauiContext?.Services.GetService<LocalizationService>();
        loc?.ApplySavedLanguage();

        // Routes
        Routing.RegisterRoute("delta-signin", typeof(DeltaSignInPage));
        Routing.RegisterRoute("qr-scan", typeof(QrScanPage));
        Routing.RegisterRoute("datamatrix-scan", typeof(DataMatrixScanPage));
        Routing.RegisterRoute("photo-upload", typeof(PhotoUploadPage));
        Routing.RegisterRoute("service", typeof(ServicePage));

        Navigating += OnShellNavigating;

        AuthService.AuthStateChanged += (_, __) => ApplyAuthVisibility();
        AuthService.LoginTypeChanged += (_, __) => ApplyToggleSignInTitle();

        ApplyToggleSignInTitle();
        ApplyAuthVisibility(initial: true);
    }

    private void ApplyAuthVisibility(bool initial = false)
    {
        var isLoggedIn = AuthService.IsLoggedIn;

        DashboardItem.IsVisible = isLoggedIn;
        BulletinsItem.IsVisible = isLoggedIn;
        ProfileItem.IsVisible = isLoggedIn;
        LogoutItem.IsVisible = isLoggedIn;

        ToggleSignInItem.IsVisible = !isLoggedIn;

        if (initial)
        {
            MainThread.BeginInvokeOnMainThread(async () =>
            {
                if (isLoggedIn)
                {
                    await GoToAsync("//dashboard");
                }
                else
                {
                    AuthService.SetLoginType(LoginType.Delta);
                    await GoToAsync("//signin");
                    ForceShowDeltaPre();
                }
            });
        }
    }

    private void ApplyToggleSignInTitle()
    {
        ToggleSignInItem.Title =
            AuthService.CurrentLoginType == LoginType.Delta
                ? "SMARTrack Sign In"
                : "Delta Sign In";
    }

    private async void OnShellNavigating(object? sender, ShellNavigatingEventArgs e)
    {
        var target = e.Target.Location.OriginalString ?? string.Empty;

        // Gate auth-only routes
        if (!AuthService.IsLoggedIn && AuthRoutes.Any(r => target.Contains(r, StringComparison.OrdinalIgnoreCase)))
        {
            e.Cancel();
            await DisplayAlert("Login required", "Please sign in to access that page.", "OK");
            await GoToAsync("//signin");
            return;
        }

        // While logged out, tapping the sign-in flyout item toggles method/page
        if (!AuthService.IsLoggedIn &&
            (target.Contains("//signin", StringComparison.OrdinalIgnoreCase) ||
             string.Equals(target.Trim('/'), "signin", StringComparison.OrdinalIgnoreCase)))
        {
            e.Cancel();
            ToggleSignInMethod();
        }
    }

    private void ToggleSignInMethod()
    {
        if (AuthService.CurrentLoginType == LoginType.Delta)
        {
            AuthService.SetLoginType(LoginType.SMARTrack);
            ForceShowSmarTrack();
        }
        else
        {
            AuthService.SetLoginType(LoginType.Delta);
            ForceShowDeltaPre();
        }

        ApplyToggleSignInTitle();
    }

    private void ForceShowDeltaPre()
    {
        SignInShellContent.ContentTemplate = new DataTemplate(typeof(DeltaPreSignInPage));
    }

    private void ForceShowSmarTrack()
    {
        SignInShellContent.ContentTemplate = new DataTemplate(typeof(SMARTrackSignInPage));
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        AuthService.Logout();

#if ANDROID
        var services = Current?.Handler?.MauiContext?.Services;
        var enableSounds = services?.GetService<SettingsService>()?.Load().EnableSounds ?? true;

        if (enableSounds)
        {
            var sound = services?.GetService<SoundService>();
            if (sound is not null)
                await sound.PlayLogoutQuitAsync();

            await Task.Delay(350);
        }

        var closer = services?.GetService<IAppCloser>();
        closer?.CloseApp();
#else
        await GoToAsync("//signin");
#endif
    }
}
