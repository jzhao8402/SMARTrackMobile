namespace SMARTrackMobile.Services;

public class ServiceAlarmScheduler
{
    public Task ScheduleAsync(DateTimeOffset targetUtc)
    {
#if ANDROID
        Platforms.Android.ServiceAlarmSchedulerAndroid.Schedule(targetUtc);
#endif
        return Task.CompletedTask;
    }

    public Task CancelAsync()
    {
#if ANDROID
        Platforms.Android.ServiceAlarmSchedulerAndroid.CancelAll();
#endif
        return Task.CompletedTask;
    }
}
