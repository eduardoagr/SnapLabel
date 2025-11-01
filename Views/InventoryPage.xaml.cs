namespace SnapLabel.Views;

public partial class InventoryPage : ContentPage {

    public InventoryPage(InventoryPageViewModel inventoryPageViewModel) {
        InitializeComponent();

        BindingContext = inventoryPageViewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args) {

        base.OnNavigatedTo(args);

        if(BindingContext is InventoryPageViewModel vm) {
            await vm.InitializeAsync();
        }
    }

}