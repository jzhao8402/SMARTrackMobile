using Android.App;
using Android.Content;
using Android.OS;

namespace SMARTrackMobile.Platforms.Android;

public static class ServiceAlarmSchedulerAndroid
{
    private const int BaseRequestCode = 3000;

    // schedule post-due alarms up to 2 hours (customize)
    private const int MaxPostAlarms = 60; // 60 * 2 minutes = 120 minutes

    public static void Schedule(DateTimeOffset targetUtc)
    {
        CancelAll();

        var context = global::Android.App.Application.Context;
        var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService)!;
        var now = DateTimeOffset.UtcNow;

        void ScheduleOne(DateTimeOffset whenUtc, string kind, int requestCode)
        {
            var intent = new Intent(context, typeof(global::SMARTrackMobile.ServiceAlarmReceiver));
            intent.SetAction(global::SMARTrackMobile.ServiceAlarmReceiver.ActionServiceAlarm);
            intent.PutExtra(global::SMARTrackMobile.ServiceAlarmReceiver.ExtraAlarmKind, kind);

            var pending = PendingIntent.GetBroadcast(
                context,
                requestCode,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            var triggerAtMillis = whenUtc.ToUnixTimeMilliseconds();

            if (Build.VERSION.SdkInt >= BuildVersionCodes.M)
                alarmManager.SetExactAndAllowWhileIdle(AlarmType.RtcWakeup, triggerAtMillis, pending);
            else
                alarmManager.SetExact(AlarmType.RtcWakeup, triggerAtMillis, pending);
        }

        var tMinus5 = targetUtc.AddMinutes(-5);
        var tMinus2 = targetUtc.AddMinutes(-2);
        var t0 = targetUtc;

        var rc = BaseRequestCode;

        if (tMinus5 > now) ScheduleOne(tMinus5, "5min", rc++);
        if (tMinus2 > now) ScheduleOne(tMinus2, "2min", rc++);
        if (t0 > now) ScheduleOne(t0, "0min", rc++);

        // Post-due: +2, +4, +6... minutes after target.
        DateTimeOffset firstPost;
        if (now <= targetUtc)
        {
            firstPost = targetUtc.AddMinutes(2);
        }
        else
        {
            var minutesPast = (now - targetUtc).TotalMinutes;
            var k = (int)Math.Floor(minutesPast / 2.0) + 1; // next bucket strictly in future
            if (k < 1) k = 1;
            firstPost = targetUtc.AddMinutes(k * 2);
        }

        for (int i = 0; i < MaxPostAlarms; i++)
        {
            var when = firstPost.AddMinutes(i * 2);
            if (when <= now) continue;
            ScheduleOne(when, "post", rc++);
        }
    }

    public static void CancelAll()
    {
        var context = global::Android.App.Application.Context;
        var alarmManager = (AlarmManager)context.GetSystemService(Context.AlarmService)!;

        for (int rc = BaseRequestCode; rc < BaseRequestCode + 500; rc++)
        {
            var intent = new Intent(context, typeof(global::SMARTrackMobile.ServiceAlarmReceiver));
            intent.SetAction(global::SMARTrackMobile.ServiceAlarmReceiver.ActionServiceAlarm);

            var pending = PendingIntent.GetBroadcast(
                context,
                rc,
                intent,
                PendingIntentFlags.UpdateCurrent | PendingIntentFlags.Immutable);

            alarmManager.Cancel(pending);
            pending.Cancel();
        }
    }
}
