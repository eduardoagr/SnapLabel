namespace SnapLabel.ViewModels {

    public partial class InventoryPageViewModel(IShellService shellService,
        IBleManager bleManager) : ObservableObject {

        private IDisposable? scanSub;

        private bool _isHandlingBluetooth;

        #region Observable Properties

        [ObservableProperty]
        public partial BluetoothDevice? Device { get; set; }

        [ObservableProperty]
        public partial string? BluetoothIcon { get; set; } = FontsConstants.Bluetooth;

        public ObservableCollection<BluetoothDevice> Devices { get; } = [];

        public ObservableCollection<Product> Products { get; set; } = [];

        [ObservableProperty]
        public partial bool IsDevicesPopupVisible { get; set; }

        [ObservableProperty]
        public partial bool IsDeviceConnectedPopupVisible { get; set; }

        [ObservableProperty]
        public partial bool IsDeviceConnected { get; set; }

        [ObservableProperty]
        public partial string DevicePopupBtonText { get; set; } = "Cancel";

        private bool IsBusy;

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
        async Task DisconnectDevice() {

            var deviceName = Device?.Name;

            Device?.Peripheral.CancelConnection();

            DeviceStatus(false, Device);

            await shellService.DisplayToastAsync($"Disconnected successfully {deviceName}");

            IsDeviceConnected = false;

            Housekeeping();

        }

        [RelayCommand]
        void CloseDevicesPopUp() {

            IsDevicesPopupVisible = false;
            Housekeeping();
        }


        [RelayCommand]
        async Task HandleBluetooth() {
            if(_isHandlingBluetooth) {
                return;
            }

            _isHandlingBluetooth = true;

            try {

                if(IsDeviceConnected) {
                    IsDeviceConnectedPopupVisible = true;
                }
                else {
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
                        if(string.IsNullOrEmpty(peripheral.Name))
                            return;

                        if(!Devices.Any(p => p.Uuid == peripheral.Uuid)) {
                            var device = new BluetoothDevice(peripheral);
                            MainThread.BeginInvokeOnMainThread(() => Devices.Add(device));
                        }
                    });
                }
            } catch(Exception ex) {

                await shellService.DisplayToastAsync($"Bluetooth error: {ex.Message}");

            } finally {
                _isHandlingBluetooth = false;
            }
        }


        [RelayCommand]
        async Task AddItem() {
            await shellService.NavigateToAsync(nameof(NewProductPage));
        }

        [RelayCommand]
        async Task DeviceSelected(BluetoothDevice? device) {

            try {

                await shellService.DisplayToastAsync($"Connecting to: {device!.Name}");

                await device.Peripheral.ConnectAsync(new ConnectionConfig {
                    AutoConnect = true,
                });

                device.Peripheral.WhenConnected().Subscribe(_ => {

                    IsDeviceConnected = true;

                    MainThread.BeginInvokeOnMainThread(() => {
                        DeviceStatus(IsDeviceConnected, device);

                    });
                });

            } catch(Exception ex) {

                await shellService.DisplayToastAsync(ex.Message);

            } finally {

            }
        }

        private void DeviceStatus(bool connected, BluetoothDevice? device) {

            if(connected) {

                DevicePopupBtonText = "Done";
                IsDevicesPopupVisible = false;
                BluetoothIcon = FontsConstants.Bluetooth_connected;
                Device = device;
            }
            else {
                BluetoothIcon = FontsConstants.Bluetooth;
                IsDeviceConnectedPopupVisible = false;
                DevicePopupBtonText = "Cancel";
                Device = null;
            }
        }

        [RelayCommand]
        async Task SendData(Product product) {

        }

        void Housekeeping() {

            scanSub?.Dispose();
            scanSub = null;

        }
        #endregion
    }
}