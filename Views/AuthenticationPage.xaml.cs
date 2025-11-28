namespace SnapLabel.Views;

public partial class AuthenticationPage : ContentPage {

    public AuthenticationPage(AuthenticationPageViewModel authenticationPageViewModel) {

        InitializeComponent();

        BindingContext = authenticationPageViewModel;

        if(DeviceInfo.Idiom == DeviceIdiom.Desktop) {

            createAccountPopUp.WidthRequest = 400;
            createAccountPopUp.HeightRequest = 500;
        }
        else {
            createAccountPopUp.AutoSizeMode = PopupAutoSizeMode.Both;
        }
    }
}