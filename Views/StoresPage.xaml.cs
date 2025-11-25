namespace SnapLabel.Views;

public partial class StoresPage : ContentPage {

    public StoresPage(StoresPageViewModel manageStoresViewModel) {
        InitializeComponent();

        BindingContext = manageStoresViewModel;
    }
}