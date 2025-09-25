namespace SnapLabel.Views;

public partial class InventoryPage : ContentPage {

    public InventoryPage(InventoryPageViewModel inventoryPageViewModel) {

        InitializeComponent();


        BindingContext = inventoryPageViewModel;

        BluetoothDiscoveryPopUp.WidthRequest = DeviceDisplay.MainDisplayInfo.Width / DeviceDisplay.MainDisplayInfo.Density * 0.35;
        BluetoothDiscoveryPopUp.HeightRequest = DeviceDisplay.MainDisplayInfo.Height / DeviceDisplay.MainDisplayInfo.Density * 0.48;


    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args) {

        if(BindingContext is InventoryPageViewModel vm) {
            await vm.GetItems();
        }
    }
}