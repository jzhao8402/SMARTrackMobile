using Plugin.Maui.Audio;

namespace SMARTrackMobile.Services;

public class SoundService
{
    private readonly IAudioManager _audioManager;

    public SoundService(IAudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    public Task PlayLoginSuccessAsync() => PlayAsync("login_success.mp3");
    public Task PlayLogoutQuitAsync() => PlayAsync("logout_quit.mp3");
    public Task PlayServiceAlarmAsync() => PlayAsync("service_alarm.mp3");

    private async Task PlayAsync(string fileName)
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync(fileName);
            var player = _audioManager.CreatePlayer(stream);
            player.Play();
        }
        catch
        {
            // best-effort
        }
    }
}
