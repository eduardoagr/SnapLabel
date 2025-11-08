namespace SnapLabel.Views;

public partial class ManageStoresPage : ContentPage {

    public ManageStoresPage(ManageStoresViewModel manageStoresViewModel) {
        InitializeComponent();

        BindingContext = manageStoresViewModel;
    }
}