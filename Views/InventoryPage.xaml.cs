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

    protected override void OnAppearing() {
        base.OnAppearing();

        if(BindingContext is InventoryPageViewModel vm) {
            // Fire and forget async call, handle exceptions as needed
            _ = vm.FetchData();
        }
    }
}