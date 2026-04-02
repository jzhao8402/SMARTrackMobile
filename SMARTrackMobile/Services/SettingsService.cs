using Microsoft.Maui.Storage;

namespace SMARTrackMobile.Services;

public class SettingsService
{
    private const string DeltaSsoUriKey = "settings_delta_sso_uri";
    private const string EnvironmentKey = "settings_environment";
    private const string EnableSoundsKey = "settings_enable_sounds";
    private const string AzureConnKey = "settings_azure_blob_conn";
    private const string AzureContainerKey = "settings_azure_blob_container";

    private const string DefaultDeltaSsoUri = "https://your-sso-host.example.com/sso/start";
    private const string DefaultEnvironment = "PROD";
    private const string DefaultAzureConn = "";
    private const string DefaultAzureContainer = "uploads";

    public event EventHandler? SettingsChanged;

    public AppSettings Load() => new(
        DeltaSsoUri: Preferences.Get(DeltaSsoUriKey, DefaultDeltaSsoUri),
        Environment: Preferences.Get(EnvironmentKey, DefaultEnvironment),
        EnableSounds: Preferences.Get(EnableSoundsKey, true),
        AzureBlobConnectionString: Preferences.Get(AzureConnKey, DefaultAzureConn),
        AzureBlobContainerName: Preferences.Get(AzureContainerKey, DefaultAzureContainer)
    );

    public void Save(AppSettings settings)
    {
        Preferences.Set(DeltaSsoUriKey, settings.DeltaSsoUri);
        Preferences.Set(EnvironmentKey, settings.Environment);
        Preferences.Set(EnableSoundsKey, settings.EnableSounds);
        Preferences.Set(AzureConnKey, settings.AzureBlobConnectionString);
        Preferences.Set(AzureContainerKey, settings.AzureBlobContainerName);

        SettingsChanged?.Invoke(this, EventArgs.Empty);
    }

    public string DebugDump()
    {
        var s = Load();
        return
            $"Env={s.Environment}\n" +
            $"DeltaSsoUri={s.DeltaSsoUri}\n" +
            $"EnableSounds={s.EnableSounds}\n" +
            $"AzureContainer={s.AzureBlobContainerName}\n" +
            $"AzureConn={(string.IsNullOrWhiteSpace(s.AzureBlobConnectionString) ? \"(empty)\" : \"(set)\")}";
    }

    /// <summary>
    /// Supported QR payload formats:
    /// JSON: {"env":"UAT","deltaSsoUri":"https://...","enableSounds":true,"azureConn":"...","azureContainer":"uploads"}
    /// Query string: env=UAT&deltaSsoUri=...&enableSounds=true&azureConn=...&azureContainer=uploads
    /// </summary>
    public bool TryApplyFromQrPayload(string payload, out string message)
    {
        payload = payload?.Trim() ?? string.Empty;
        if (payload.Length == 0)
        {
            message = "QR code is empty.";
            return false;
        }

        // JSON
        if (payload.StartsWith("{"))
        {
            try
            {
                var doc = System.Text.Json.JsonDocument.Parse(payload);
                var root = doc.RootElement;

                var current = Load();

                var env = current.Environment;
                var delta = current.DeltaSsoUri;
                var sounds = current.EnableSounds;
                var azureConn = current.AzureBlobConnectionString;
                var azureContainer = current.AzureBlobContainerName;

                if (root.TryGetProperty("env", out var envProp) && envProp.ValueKind == System.Text.Json.JsonValueKind.String)
                    env = envProp.GetString() ?? env;

                if (root.TryGetProperty("deltaSsoUri", out var uriProp) && uriProp.ValueKind == System.Text.Json.JsonValueKind.String)
                    delta = uriProp.GetString() ?? delta;

                if (root.TryGetProperty("enableSounds", out var sndProp) &&
                    (sndProp.ValueKind == System.Text.Json.JsonValueKind.True || sndProp.ValueKind == System.Text.Json.JsonValueKind.False))
                    sounds = sndProp.GetBoolean();

                if (root.TryGetProperty("azureConn", out var ac) && ac.ValueKind == System.Text.Json.JsonValueKind.String)
                    azureConn = ac.GetString() ?? azureConn;

                if (root.TryGetProperty("azureContainer", out var cont) && cont.ValueKind == System.Text.Json.JsonValueKind.String)
                    azureContainer = cont.GetString() ?? azureContainer;

                Save(new AppSettings(delta, env, sounds, azureConn, azureContainer));
                message = "Settings updated from QR (JSON).";
                return true;
            }
            catch (Exception ex)
            {
                message = $"Invalid JSON QR payload: {ex.Message}";
                return false;
            }
        }

        // Query string
        try
        {
            var dict = ParseQueryString(payload);
            var current = Load();

            var env = dict.TryGetValue("env", out var e) ? e : current.Environment;
            var delta = dict.TryGetValue("deltaSsoUri", out var d) ? d : current.DeltaSsoUri;

            var sounds = current.EnableSounds;
            if (dict.TryGetValue("enableSounds", out var s) && bool.TryParse(s, out var b))
                sounds = b;

            var azureConn = dict.TryGetValue("azureConn", out var ac) ? ac : current.AzureBlobConnectionString;
            var azureContainer = dict.TryGetValue("azureContainer", out var c) ? c : current.AzureBlobContainerName;

            Save(new AppSettings(delta, env, sounds, azureConn, azureContainer));
            message = "Settings updated from QR (query string).";
            return true;
        }
        catch (Exception ex)
        {
            message = $"Invalid QR payload: {ex.Message}";
            return false;
        }
    }

    private static Dictionary<string, string> ParseQueryString(string payload)
    {
        var q = payload;
        var idx = payload.IndexOf('?');
        if (idx >= 0 && idx < payload.Length - 1)
            q = payload[(idx + 1)..];

        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        foreach (var part in q.Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var kv = part.Split('=', 2);
            var key = Uri.UnescapeDataString(kv[0]);
            var val = kv.Length > 1 ? Uri.UnescapeDataString(kv[1]) : "";
            if (key.Length > 0)
                result[key] = val;
        }

        return result;
    }
}
