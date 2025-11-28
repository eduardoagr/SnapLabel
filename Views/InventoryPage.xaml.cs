namespace SnapLabel.Views;

public partial class InventoryPage : ContentPage {

    public InventoryPage(InventoryPageViewModel inventoryPageViewModel) {
        InitializeComponent();

        BindingContext = inventoryPageViewModel;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args) {
        if(BindingContext is InventoryPageViewModel viewModel) {
            await viewModel.InitializeAsync();
        }

    }
}