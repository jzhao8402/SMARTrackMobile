namespace SMARTrackMobile.Views.Pages;

public partial class BulletinsPage : ContentPage
{
    public BulletinsPage() => InitializeComponent();

    private async void OnFilterClicked(object sender, EventArgs e)
        => await DisplayAlert("Bulletins", "Filter clicked.", "OK");

    private async void OnRefreshClicked(object sender, EventArgs e)
        => await DisplayAlert("Bulletins", "Refresh clicked.", "OK");

    private async void OnHelpClicked(object sender, EventArgs e)
        => await DisplayAlert("Bulletins", "Help clicked.", "OK");
}
