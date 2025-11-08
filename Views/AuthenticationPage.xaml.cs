namespace SnapLabel.Views;

public partial class AuthenticationPage : ContentPage {

    public AuthenticationPage(AuthenticationPageViewModel authenticationPageViewModel) {
        InitializeComponent();

        BindingContext = authenticationPageViewModel;
    }
}