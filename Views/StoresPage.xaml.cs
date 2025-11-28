namespace SnapLabel.Views;

public partial class StoresPage : ContentPage {

    public StoresPage(StoresPageViewMode manageStoresViewModel) {
        InitializeComponent();

        BindingContext = manageStoresViewModel;
    }
}