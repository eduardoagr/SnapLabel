namespace SnapLabel.Views;

public partial class StoresPage : ContentPage {

    public StoresPage(StoresViewModel manageStoresViewModel) {
        InitializeComponent();

        BindingContext = manageStoresViewModel;
    }
}