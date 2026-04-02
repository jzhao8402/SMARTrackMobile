using SMARTrackMobile.Services;

namespace SMARTrackMobile.Views.Pages;

public partial class SMARTrackSignInPage : ContentPage
{
    public SMARTrackSignInPage()
    {
        InitializeComponent();
        AuthService.SetLoginType(LoginType.SMARTrack);
    }

    private async void OnSignInClicked(object sender, EventArgs e)
    {
        AuthService.Login(LoginType.SMARTrack);

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
}
