using SMARTrackMobile.Services;

namespace SMARTrackMobile.Views.Pages;

public partial class ServicePage : ContentPage
{
    private readonly ServerTimeService _serverTime;
    private readonly ServiceAlarmScheduler _alarmScheduler;
    private readonly SoundService _sound;
    private readonly SettingsService _settings;

    private DateTimeOffset _targetUtc;
    private CancellationTokenSource? _cts;

    private bool _firedMinus5;
    private bool _firedMinus2;
    private bool _firedAt0;
    private int _lastPostK = 0;

    public ServicePage()
    {
        InitializeComponent();

        var services = Shell.Current?.Handler?.MauiContext?.Services
                       ?? throw new InvalidOperationException("Maui services not available.");

        _serverTime = services.GetService<ServerTimeService>()
                      ?? throw new InvalidOperationException("ServerTimeService not available.");
        _alarmScheduler = services.GetService<ServiceAlarmScheduler>()
                         ?? throw new InvalidOperationException("ServiceAlarmScheduler not available.");
        _sound = services.GetService<SoundService>()
                 ?? throw new InvalidOperationException("SoundService not available.");
        _settings = services.GetService<SettingsService>()
                    ?? throw new InvalidOperationException("SettingsService not available.");
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await SyncFromServerAsync();
        StartUiLoop();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        StopUiLoop();
    }

    private async Task SyncFromServerAsync()
    {
        _targetUtc = await _serverTime.GetTargetTimestampUtcAsync();
        TargetLabel.Text = $"Target (UTC): {_targetUtc:yyyy-MM-dd HH:mm:ss}";
        ResetAlarmFlags();
        UpdateUi();
    }

    private void ResetAlarmFlags()
    {
        _firedMinus5 = _firedMinus2 = _firedAt0 = false;
        _lastPostK = 0;
    }

    private void StartUiLoop()
    {
        StopUiLoop();
        _cts = new CancellationTokenSource();
        _ = RunLoopAsync(_cts.Token);
    }

    private void StopUiLoop()
    {
        _cts?.Cancel();
        _cts = null;
    }

    private async Task RunLoopAsync(CancellationToken ct)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
        while (!ct.IsCancellationRequested)
        {
            UpdateUi();
            MaybeFireInAppAlarms();

            try { await timer.WaitForNextTickAsync(ct); }
            catch { break; }
        }
    }

    private void UpdateUi()
    {
        var now = DateTimeOffset.UtcNow;
        var diff = _targetUtc - now;

        var abs = diff.Duration();
        var mmss = $"{(int)abs.TotalMinutes:00}:{abs.Seconds:00}";

        if (diff >= TimeSpan.Zero)
        {
            TimeLeftLabel.Text = mmss;
            StatusLabel.Text = "Time remaining";
        }
        else
        {
            TimeLeftLabel.Text = "-" + mmss;
            StatusLabel.Text = "Past target";
        }
    }

    private void MaybeFireInAppAlarms()
    {
        var enableSounds = _settings.Load().EnableSounds;
        if (!enableSounds) return;

        var now = DateTimeOffset.UtcNow;
        var remaining = _targetUtc - now;

        // T-5 (only if we're still above T-2 when we cross it)
        if (!_firedMinus5 && remaining <= TimeSpan.FromMinutes(5) && remaining > TimeSpan.FromMinutes(2))
        {
            _firedMinus5 = true;
            _ = _sound.PlayServiceAlarmAsync();
        }

        // T-2 (only if still above 0)
        if (!_firedMinus2 && remaining <= TimeSpan.FromMinutes(2) && remaining > TimeSpan.Zero)
        {
            _firedMinus2 = true;
            _ = _sound.PlayServiceAlarmAsync();
        }

        // T
        if (!_firedAt0 && remaining <= TimeSpan.Zero)
        {
            _firedAt0 = true;
            _ = _sound.PlayServiceAlarmAsync();
        }

        // Post due alarms: +2, +4, +6...
        if (remaining < TimeSpan.Zero)
        {
            var minutesPast = (now - _targetUtc).TotalMinutes;
            var k = (int)Math.Floor(minutesPast / 2.0); // 1=>+2..+4, 2=>+4..+6 ...

            if (k >= 1 && k != _lastPostK)
            {
                _lastPostK = k;
                _ = _sound.PlayServiceAlarmAsync();
            }
        }
    }

    private async void OnStartClicked(object sender, EventArgs e)
    {
        await _alarmScheduler.ScheduleAsync(_targetUtc);
        await DisplayAlert("Scheduled", "Android alarms scheduled (T-5, T-2, T, then every +2 min).", "OK");
    }

    private async void OnStopClicked(object sender, EventArgs e)
    {
        await _alarmScheduler.CancelAsync();
        await DisplayAlert("Canceled", "Android alarms canceled.", "OK");
    }

    private async void OnResyncClicked(object sender, EventArgs e)
    {
        await SyncFromServerAsync();
        await DisplayAlert("Resynced", "Fetched target timestamp again.", "OK");
    }
}
