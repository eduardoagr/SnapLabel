namespace SnapLabel.Views;

public partial class AuthenticationPage : ContentPage {

    public AuthenticationPage(AuthenticationPageViewModel authenticationPageViewModel) {
        InitializeComponent();

        BindingContext = authenticationPageViewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args) {
        base.OnNavigatedTo(args);

        if(BindingContext is AuthenticationPageViewModel vm) {
            // await vm.CheckAuth();
        }
    }
}