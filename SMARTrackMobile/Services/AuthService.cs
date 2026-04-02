using Microsoft.Maui.Storage;

namespace SMARTrackMobile.Services;

public static class AuthService
{
    private const string IsLoggedInKey = "auth_is_logged_in";
    private const string LoginTypeKey = "auth_login_type";

    public static bool IsLoggedIn
    {
        get => Preferences.Get(IsLoggedInKey, false);
        private set => Preferences.Set(IsLoggedInKey, value);
    }

    public static LoginType CurrentLoginType
    {
        get => (LoginType)Preferences.Get(LoginTypeKey, (int)LoginType.Delta);
        private set => Preferences.Set(LoginTypeKey, (int)value);
    }

    public static event EventHandler? AuthStateChanged;
    public static event EventHandler? LoginTypeChanged;

    public static void SetLoginType(LoginType type)
    {
        if (CurrentLoginType == type) return;
        CurrentLoginType = type;
        LoginTypeChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void Login(LoginType type)
    {
        CurrentLoginType = type;
        IsLoggedIn = true;

        LoginTypeChanged?.Invoke(null, EventArgs.Empty);
        AuthStateChanged?.Invoke(null, EventArgs.Empty);
    }

    public static void Logout()
    {
        IsLoggedIn = false;

        // Always default to Delta when logged out
        CurrentLoginType = LoginType.Delta;
        LoginTypeChanged?.Invoke(null, EventArgs.Empty);

        AuthStateChanged?.Invoke(null, EventArgs.Empty);
    }
}
