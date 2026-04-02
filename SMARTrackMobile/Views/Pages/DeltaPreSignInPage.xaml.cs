using SMARTrackMobile.Services;

namespace SMARTrackMobile.Views.Pages;

public partial class DeltaPreSignInPage : ContentPage
{
    public DeltaPreSignInPage()
    {
        InitializeComponent();
        AuthService.SetLoginType(LoginType.Delta);
    }

    private async void OnDeltaSignInClicked(object sender, EventArgs e)
    {
        AuthService.SetLoginType(LoginType.Delta);
        await Shell.Current.GoToAsync("delta-signin");
    }
}
