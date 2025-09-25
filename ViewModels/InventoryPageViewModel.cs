namespace SnapLabel.ViewModels {

    public partial class InventoryPageViewModel : ObservableObject {

        #region Readonly and Static Fields
        private readonly IShellService _shellService;
        private readonly DatabaseService _databaseService;
        private readonly IBluetoothService? _bluetoothService;
        #endregion

        #region Observable Properties
        [ObservableProperty]
        public partial bool IsPopUpOpen { get; set; }

        public ObservableCollection<Product> Products { get; set; } = [];

        public ObservableCollection<BluetoothDeviceModel> Devices { get; set; } = [];

        [ObservableProperty]
        public partial BluetoothDeviceModel? SelectedDevice { get; set; }
        #endregion

        #region Constructor
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

        #endregion

        #region Methods and Commands
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
        void ClosePopUp() {

            IsPopUpOpen = false;
            _bluetoothService?.StopScan();
        }

        [RelayCommand]
        void ScanBluetooth() {

            IsPopUpOpen = true;
            Devices.Clear();
            _bluetoothService?.StartScan();

        }

        [RelayCommand]
        public async Task AddItem() {
            await _shellService.NavigateToAsync(nameof(NewProductPage));
        }
        #endregion

        [RelayCommand]
        void DeviceSelected(BluetoothDeviceModel bluetoothDeviceModel) {


        }
    }

}
