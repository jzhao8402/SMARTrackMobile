namespace SMARTrackMobile.Services;

public class ServerTimeService
{
    // TODO: replace with real server API call returning target timestamp (UTC)
    public Task<DateTimeOffset> GetTargetTimestampUtcAsync()
        => Task.FromResult(DateTimeOffset.UtcNow.AddMinutes(20));
}
