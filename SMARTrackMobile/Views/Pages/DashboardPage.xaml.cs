namespace SMARTrackMobile.Views.Pages;

public partial class DashboardPage : ContentPage
{
    public DashboardPage() => InitializeComponent();

    private async void OnRefreshClicked(object sender, EventArgs e)
        => await DisplayAlert("Dashboard", "Refresh clicked.", "OK");

    private async void OnHelpClicked(object sender, EventArgs e)
        => await DisplayAlert("Dashboard", "Help clicked.", "OK");

    private async void OnOpenServiceClicked(object sender, EventArgs e)
        => await Shell.Current.GoToAsync("service");
}
