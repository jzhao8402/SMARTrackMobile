namespace SMARTrackMobile.Views.Pages;

public partial class ProfilePage : ContentPage
{
    public ProfilePage() => InitializeComponent();

    private async void OnEditClicked(object sender, EventArgs e)
        => await DisplayAlert("Profile", "Edit clicked.", "OK");

    private async void OnRefreshClicked(object sender, EventArgs e)
        => await DisplayAlert("Profile", "Refresh clicked.", "OK");

    private async void OnHelpClicked(object sender, EventArgs e)
        => await DisplayAlert("Profile", "Help clicked.", "OK");
}
