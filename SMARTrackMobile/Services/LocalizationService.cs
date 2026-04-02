using System.Globalization;
using Microsoft.Maui.Storage;

namespace SMARTrackMobile.Services;

public class LocalizationService
{
    private const string LanguageKey = "settings_language"; // "en" or "es"
    public string CurrentLanguage => Preferences.Get(LanguageKey, "en");

    public void ApplySavedLanguage() => ApplyLanguage(CurrentLanguage);

    public void SetLanguage(string languageCode)
    {
        if (languageCode is not ("en" or "es"))
            languageCode = "en";

        Preferences.Set(LanguageKey, languageCode);
        ApplyLanguage(languageCode);
    }

    private static void ApplyLanguage(string languageCode)
    {
        var culture = new CultureInfo(languageCode);
        CultureInfo.DefaultThreadCurrentCulture = culture;
        CultureInfo.DefaultThreadCurrentUICulture = culture;
        CultureInfo.CurrentCulture = culture;
        CultureInfo.CurrentUICulture = culture;
    }
}
