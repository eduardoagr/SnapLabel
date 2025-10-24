using System.Reactive.Linq;

namespace SnapLabel.ViewModels {

    // ViewModel for managing Bluetooth devices and product printing
    public partial class InventoryPageViewModel(IShellService shellService, IBleManager bleManager) : ObservableObject {

        // Subscription to BLE scan stream
        private IDisposable? scanSub;

        // Prevents concurrent Bluetooth handling
        private bool _isHandlingBluetooth;

        // Tracks whether disconnect was user-initiated
        bool userInitiatedDisconnect = true;

        #region Observable Properties

        // Currently connected Bluetooth device
        [ObservableProperty]
        public partial BluetoothDevice? Device { get; set; }

        // Icon representing Bluetooth connection state
        [ObservableProperty]
        public partial string? BluetoothIcon { get; set; } = FontsConstants.Bluetooth;

        // List of discovered Bluetooth devices
        public ObservableCollection<BluetoothDevice> Devices { get; } = [];

        // List of products to be printed
        public ObservableCollection<Product> Products { get; set; } = [];

        // Controls visibility of device selection popup
        [ObservableProperty]
        public partial bool IsDevicesPopupVisible { get; set; }

        // Controls visibility of connected device popup
        [ObservableProperty]
        public partial bool IsDeviceConnectedPopupVisible { get; set; }

        // Controls visibility of printing progress popup
        [ObservableProperty]
        public partial bool IsPrintingPopUoVisible { get; set; }

        #endregion

        #region Methods and Commands

        /// <summary>
        /// Initializes the view model and loads product data.
        /// </summary>
        public async Task InitializeAsync() {
            // TODO: Load products from database
        }

        /// <summary>
        /// Disconnects from the current Bluetooth device.
        /// </summary>
        [RelayCommand]
        async Task DisconnectDevice() {
            var deviceName = Device?.Name;

            userInitiatedDisconnect = true;
            Device?.Peripheral.CancelConnection();
            Housekeeping();
        }

        /// <summary>
        /// Closes the device selection popup.
        /// </summary>
        [RelayCommand]
        void CloseDevicesPopUp() {
            IsDevicesPopupVisible = false;
            Housekeeping();
        }

        /// <summary>
        /// Handles Bluetooth access and device scanning.
        /// </summary>
        [RelayCommand]
        async Task HandleBluetooth() {
            if(_isHandlingBluetooth)
                return;

            _isHandlingBluetooth = true;

            try {
                // Already connected — show connected popup
                if(Device != null && Device.Peripheral.Status == ConnectionState.Connected) {
                    IsDeviceConnectedPopupVisible = true;
                }
                else {
                    // Request Bluetooth access
                    var access = await bleManager.RequestAccessAsync();
                    if(access != Shiny.AccessState.Available) {
                        await shellService.DisplayAlertAsync("Error", "Bluetooth is not available. Please enable it.", "OK");
                        return;
                    }

                    // Show device selection and start scanning
                    IsDevicesPopupVisible = true;
                    Devices.Clear();

                    await StartScanningAsync();
                }
            } catch(Exception ex) {
                await shellService.DisplayToastAsync($"Bluetooth error: {ex.Message}");
            } finally {
                _isHandlingBluetooth = false;
            }
        }

        /// <summary>
        /// Starts scanning for nearby BLE devices.
        /// </summary>
        private async Task StartScanningAsync() {
            try {
                scanSub = bleManager.Scan().Subscribe(scan => {
                    var peripheral = scan.Peripheral;
                    if(string.IsNullOrEmpty(peripheral.Name))
                        return;

                    // Add device if not already listed
                    if(!Devices.Any(p => p.Uuid == peripheral.Uuid)) {
                        var device = new BluetoothDevice(peripheral);
                        MainThread.BeginInvokeOnMainThread(() => Devices.Add(device));
                    }
                });
            } catch(Exception ex) {
                await shellService.DisplayToastAsync($"Scan failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Navigates to the product creation page.
        /// </summary>
        [RelayCommand]
        async Task AddItem() {
            await shellService.NavigateToAsync(nameof(NewProductPage));
        }

        /// <summary>
        /// Connects to the selected Bluetooth device.
        /// </summary>
        [RelayCommand]
        async Task ConnectToDeviceAsync(BluetoothDevice? device) {
            if(device == null)
                return;

            try {
                await shellService.DisplayToastAsync($"Connecting to: {device.Name}");

                await device.Peripheral.ConnectAsync(new ConnectionConfig { AutoConnect = true });

                // Wait for connection confirmation
                await device.Peripheral.WhenConnected().FirstAsync();

                // Update state and subscribe to future changes
                await UpdateDeviceConnectionState(ConnectionState.Connected, device);

                device.Peripheral.WhenStatusChanged().Subscribe(status => {
                    MainThread.BeginInvokeOnMainThread(async () => {
                        await UpdateDeviceConnectionState(status, device);
                    });
                });
            } catch(Exception ex) {
                await shellService.DisplayToastAsync($"Failed to connect to {device.Name}: {ex.Message}");
            }
        }

        /// <summary>
        /// Updates UI and state based on connection status.
        /// </summary>
        private async Task UpdateDeviceConnectionState(ConnectionState state, BluetoothDevice? device) {
            var reason = userInitiatedDisconnect
                ? "Disconnected manually by user"
                : device?.Peripheral.Status switch {
                    ConnectionState.Disconnected => "Device powered off or out of range",
                    ConnectionState.Connecting => "Connection interrupted",
                    ConnectionState.Connected => "Unexpected disconnect",
                    _ => "Unknown"
                };

            switch(state) {
                case ConnectionState.Disconnected:
                    BluetoothIcon = FontsConstants.Bluetooth;
                    IsDeviceConnectedPopupVisible = false;
                    await shellService.DisplayToastAsync($"Disconnected from {device?.Name}. Reason: {reason}");
                    Housekeeping();
                    userInitiatedDisconnect = false;
                    Device = null;
                    break;

                case ConnectionState.Disconnecting:
                    BluetoothIcon = FontsConstants.Bluetooth;
                    await shellService.DisplayToastAsync($"Disconnecting from {device?.Name}...");
                    break;

                case ConnectionState.Connected:
                    IsDevicesPopupVisible = false;
                    BluetoothIcon = FontsConstants.Bluetooth_connected;
                    Device = device;
                    await shellService.DisplayToastAsync($"Connected to {device?.Name} successfully");
                    break;

                case ConnectionState.Connecting:
                    BluetoothIcon = FontsConstants.Bluetooth;
                    await shellService.DisplayToastAsync($"Connecting to {device?.Name}...");
                    break;

                default:
                    break;
            }
        }

        /// <summary>
        /// Sends a print job to the connected device.
        /// </summary>
        [RelayCommand]
        async Task SendData(Product product) {
            if(Device == null || Device.Peripheral.Status == ConnectionState.Disconnected) {
                await shellService.DisplayToastAsync("No device connected.");
                return;
            }

            IsPrintingPopUoVisible = true;

            bool isFinished = await Printer.PrintTextAsync(Device.Peripheral, "Hello");

            if(isFinished) {
                IsPrintingPopUoVisible = false;
            }
        }

        /// <summary>
        /// Cleans up scanning subscription.
        /// </summary>
        private void Housekeeping() {
            scanSub?.Dispose();
            scanSub = null;
        }

        #endregion
    }
}
