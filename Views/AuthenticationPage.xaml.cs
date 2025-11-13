namespace SnapLabel.Views;

public partial class AuthenticationPage : ContentPage {

    public AuthenticationPage(AuthenticationPageViewModel authenticationPageViewModel) {
        InitializeComponent();

        BindingContext = authenticationPageViewModel;

        Loaded += async (sender, args) => {

            if(BindingContext is AuthenticationPageViewModel vm) {

                //await vm.CheckAuth();
            }
        };
    }
}