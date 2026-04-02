using Android.App;
using Android.Content;
using Android.OS;

namespace SMARTrackMobile;

[BroadcastReceiver(Enabled = true, Exported = false)]
public class ServiceAlarmReceiver : BroadcastReceiver
{
    public const string ActionServiceAlarm = "SMARTrackMobile.ACTION_SERVICE_ALARM";
    public const string ExtraAlarmKind = "alarm_kind"; // "5min" | "2min" | "0min" | "post"

    private const string ChannelId = "service_alarm_channel";

    public override void OnReceive(Context? context, Intent? intent)
    {
        if (context is null || intent is null)
            return;

        if (intent.Action != ActionServiceAlarm)
            return;

        var kind = intent.GetStringExtra(ExtraAlarmKind) ?? "alarm";

        EnsureChannel(context);

        var text = kind switch
        {
            "5min" => "5 minutes remaining",
            "2min" => "2 minutes remaining",
            "0min" => "Time reached",
            "post" => "Past due (2-min interval)",
            _ => "Alarm"
        };

        var notification = new Notification.Builder(context, ChannelId)
            .SetContentTitle("Service Alarm")
            .SetContentText(text)
            .SetSmallIcon(Android.Resource.Drawable.IcDialogAlert)
            .SetAutoCancel(true)
            .Build();

        var manager = (NotificationManager)context.GetSystemService(Context.NotificationService)!;
        manager.Notify((kind + DateTimeOffset.UtcNow.ToUnixTimeSeconds()).GetHashCode(), notification);
    }

    private static void EnsureChannel(Context context)
    {
        if (Build.VERSION.SdkInt < BuildVersionCodes.O)
            return;

        var manager = (NotificationManager)context.GetSystemService(Context.NotificationService)!;
        var channel = new NotificationChannel(ChannelId, "Service Alarms", NotificationImportance.High);
        manager.CreateNotificationChannel(channel);
    }
}
