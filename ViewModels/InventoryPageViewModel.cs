
namespace SnapLabel.ViewModels {

    public partial class InventoryPageViewModel : ObservableObject {

        private readonly IShellService _shellService;
        private readonly DatabaseService _databaseService;
        private readonly IBluetoothService _bluetoothService;

        [ObservableProperty]
        public partial bool Scanning { get; set; }

        [ObservableProperty]
        public partial bool IsPopUpOpen { get; set; }

        public ObservableCollection<Product> Products { get; set; } = [];

        public ObservableCollection<BluetoothDeviceModel> Devices { get; set; } = [];



        public InventoryPageViewModel(
            IShellService shellService,
            DatabaseService databaseService,
            IBluetoothService bluetoothService) {

            _shellService = shellService;
            _databaseService = databaseService;
            _bluetoothService = bluetoothService;

            _bluetoothService.DeviceFound += device => {
                MainThread.BeginInvokeOnMainThread(() => {

                    MainThread.BeginInvokeOnMainThread(() => {
                        Devices.Add(device);
                    });

                });
            };
        }


        public async Task GetItems() {
            var items = await _databaseService.GetItemsAsync();

            Products.Clear();

            if(items.Count > 0) {
                foreach(var item in items) {
                    Products.Add(item);
                }
            }
        }


        [RelayCommand]
        void ScanBluetooth() {

            IsPopUpOpen = true;
            Devices.Clear();
            _bluetoothService.StartScan();
            Scanning = true;

        }

        [RelayCommand]
        public async Task AddItem() {
            await _shellService.NavigateToAsync(nameof(NewProductPage));
        }
    }
}
