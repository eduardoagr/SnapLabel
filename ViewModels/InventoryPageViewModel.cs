
namespace SnapLabel.ViewModels {

    public partial class InventoryPageViewModel(IShellService shellService,
        IBleManager bleManager) : ObservableObject {

        private IDisposable? scanSub;

        #region Observable Properties

        [ObservableProperty]
        public partial string? bluetoothIcon { get; set; } = FontsConstants.Bluetooth;

        public ObservableCollection<BluetoothDevice> Devices { get; } = [];

        public ObservableCollection<Product> Products { get; set; } = [];

        [ObservableProperty]
        public partial bool IsDevicesPopupVisible { get; set; }

        [ObservableProperty]
        public partial bool IsDeviceConnectedPopupVisible { get; set; }

        [ObservableProperty]
        public partial bool IsRadyToPrint { get; set; }

        [ObservableProperty]
        public partial string DevicePopupBtonText { get; set; } = "Cancel";

        #endregion

        #region Methods and Commands
        public async Task InitializeAsync() {

            ////var items = await databaseService.GetItemsAsync();

            ////if(items.Count > 0) {
            ////    foreach(var item in items) {
            ////        Products.Add(item);
            ////    }
            ////}
        }

        [RelayCommand]
        void DisconnectDevice() {

        }

        [RelayCommand]
        void CloseDevicesPopUp() {

            IsDevicesPopupVisible = false;


            scanSub?.Dispose();
            scanSub = null;


        }

        [RelayCommand]
        async Task HandleBluetooth() {

            var access = await bleManager.RequestAccessAsync();
            if(access != Shiny.AccessState.Available) {
                await shellService.DisplayAlertAsync("Error",
                    "Bluetooth is not available. Please enable it.", "OK");
                return;
            }

            IsDevicesPopupVisible = true;

            Devices.Clear();
            scanSub = bleManager.Scan().Subscribe(scan => {

                var peripheral = scan.Peripheral;

                if(string.IsNullOrEmpty(peripheral.Name)) {
                    return;
                }

                // Avoid duplicates
                if(!Devices.Any(p => p.Uuid == peripheral.Uuid)) {
                    var device = new BluetoothDevice(peripheral);
                    MainThread.BeginInvokeOnMainThread(() => {
                        Devices.Add(device);
                    });
                }
            });

        }




        [RelayCommand]
        async Task AddItem() {
            await shellService.NavigateToAsync(nameof(NewProductPage));
        }

        [RelayCommand]
        async Task DeviceSelected(Peripheral peripheral) {



        }


        [RelayCommand]
        async Task SendData(Product product) {




        }
        #endregion
    }
}