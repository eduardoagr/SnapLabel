using CommunityToolkit.Mvvm.Messaging;

namespace SnapLabel.ViewModels {

    public partial class InventoryPageViewModel : ObservableObject {

        #region Readonly and Static Fields
        private readonly IShellService _shellService;
        private readonly DatabaseService _databaseService;
        private readonly IBluetoothService? _bluetoothService;
        private readonly IPreferences _preferences;
        #endregion

        #region Observable Properties

        [ObservableProperty]
        public partial BluetoothDeviceModel? BluetoothDevice { get; set; }

        public ObservableCollection<Product> Products { get; set; } = [];

        public ObservableCollection<BluetoothDeviceModel> Devices { get; set; } = [];

        [ObservableProperty]
        public partial bool IsDevicesPopupVisible { get; set; }

        [ObservableProperty]
        public partial bool IsDeviceConnectedPopupVisible { get; set; }

        [ObservableProperty]
        public partial bool IsRadyToPrint { get; set; }

        [ObservableProperty]
        public partial string DevicePopupBtonText { get; set; } = "Cancel";
        #endregion

        #region Constructor
        public InventoryPageViewModel(
            IShellService shellService,
            IPreferences preferences,
            IBluetoothService bluetoothService,
            DatabaseService databaseService) {

            _shellService = shellService;
            _databaseService = databaseService;
            _bluetoothService = bluetoothService;
            _preferences = preferences;

            _bluetoothService.DeviceFound += device => {
                MainThread.BeginInvokeOnMainThread(() => {

                    MainThread.BeginInvokeOnMainThread(() => {
                        Devices.Add(device);
                    });

                });
            };

            _bluetoothService.DeviceDisconnected += () => {
                MainThread.BeginInvokeOnMainThread(() => {
                    BluetoothDevice?.Status = DeviceConectionStatusEnum.Disconnected.ToDisplayString();
                });
            };

            WeakReferenceMessenger.Default.Register<BluetoothDeviceMessage>(this, (r, message) => {

                var device = message.Value;

                MainThread.BeginInvokeOnMainThread(async () => {
                    switch(device.Status) {

                        case var s when s == DeviceConectionStatusEnum.Connected.ToDisplayString():

                            await _shellService.DisplayToast($"Device connected successfully to {device.Name}",
                               ToastDuration.Short);

                            IsRadyToPrint = true;
                            BluetoothDevice = device;
                            DevicePopupBtonText = "Done";
                            break;

                        case var s when s == DeviceConectionStatusEnum.Failed.ToDisplayString():
                            IsRadyToPrint = false;
                            BluetoothDevice = null;
                            break;

                        case var s when s == DeviceConectionStatusEnum.Disconnected.ToDisplayString():
                            IsRadyToPrint = false;
                            BluetoothDevice = null;
                            break;

                        case var s when s == DeviceConectionStatusEnum.Connecting.ToDisplayString():
                            // Optional: show loading spinner
                            break;
                    }
                });
            });
        }

        #endregion

        #region Methods and Commands
        public async Task InitializeAsync() {
            var items = await _databaseService.GetItemsAsync();

            Products.Clear();

            if(items.Count > 0) {
                foreach(var item in items) {
                    Products.Add(item);
                }

                var connectedDevice = Operations.LoadDeviceFromPreferences(
                    _preferences);

                if(connectedDevice is not null) {

                    await AssignAndConnectAsync(connectedDevice);

                }
            }
        }

        [RelayCommand]
        void DisconnectDevice() {

            IsDeviceConnectedPopupVisible = false;

            var device = BluetoothDevice; // snapshot
            if(device is null)
                return;

            _bluetoothService?.Disconnect();

            device.Status = DeviceConectionStatusEnum.Disconnected.ToDisplayString();

            _preferences.Clear();

        }

        [RelayCommand]
        void ClosePopUp() {

            IsDevicesPopupVisible = false;
            _bluetoothService?.StopScan();
        }

        [RelayCommand]
        void ScanBluetooth() {

            if(BluetoothDevice is not null) {
                IsDeviceConnectedPopupVisible = true;
            }
            else {
                IsDevicesPopupVisible = true;
                Devices.Clear();
                _bluetoothService?.StartScan();
            }
        }

        [RelayCommand]
        public async Task AddItem() {
            await _shellService.NavigateToAsync(nameof(NewProductPage));
        }


        [RelayCommand]
        async Task DeviceSelected(BluetoothDeviceModel bluetoothDeviceModel) {

            if(bluetoothDeviceModel is not null) {

                bluetoothDeviceModel.Status = DeviceConectionStatusEnum.Connecting.ToDisplayString();

                await AssignAndConnectAsync(bluetoothDeviceModel);

                IsDevicesPopupVisible = false;
            }


        }

        private async Task AssignAndConnectAsync(BluetoothDeviceModel device) {
            var isConnected = await _bluetoothService!.ConnectAsync(device.DeviceId);
            device.Status = isConnected
                ? DeviceConectionStatusEnum.Connected.ToDisplayString()
                : DeviceConectionStatusEnum.Failed.ToDisplayString();

            if(isConnected && !Operations.IsDeviceAlreadySaved(_preferences, device)) {

                Operations.SavePreferenceInJson(_preferences, device);

            }
            #endregion
        }
    }
}