using Microsoft.IdentityModel.Tokens;

namespace SnapLabel.ViewModels;

/// <summary>
/// ViewModel responsible for Bluetooth device management,
/// connection handling, and product printing logic.
/// </summary>
///
public partial class InventoryPageViewModel(
    IShellService shellService,
    IBleManager bleManager,
    IFirebaseAuthClient firebaseAuthClient,
    IDatabaseService<Product> databaseService,
    IDatabaseService<User> UserDb,
    ICustomDialogService customDialogService,
    IMessenger messenger) : BasePageViewModel<Product>(shellService, firebaseAuthClient, databaseService, customDialogService, messenger) {


    #region 🔧 Internal State

    // Subscription reference to the BLE scanning stream (used for cleanup).
    private IDisposable? scanSub;

    // Ensures Bluetooth operations (connect/scan) aren't executed in parallel.
    private bool _isHandlingBluetooth;

    // Tracks whether a disconnect was triggered by the user (vs. lost signal).
    private bool userInitiatedDisconnect;

    #endregion

    #region 📦 Observable Properties (UI-bound)
    // --------------------------------------------------------------------
    // These properties are bound to the UI and automatically notify changes
    // --------------------------------------------------------------------


    // List of discovered Bluetooth devices shown in the device selection popup.
    public ObservableCollection<BluetoothDevice> Devices { get; } = [];

    // List of available products to print.
    public ObservableCollection<Product> Products { get; set; } = [];

    // Controls visibility of the "select Bluetooth device" popup.
    [ObservableProperty]
    public partial bool IsDevicesPopupVisible { get; set; }

    // Controls visibility of the "connected device" popup (e.g. for disconnect).
    [ObservableProperty]
    public partial bool IsDeviceConnectedPopupVisible { get; set; }

    // Icon that represents the Bluetooth connection state (connected/disconnected).
    [ObservableProperty]
    public partial string? BluetoothIcon { get; set; } = FontsConstants.Bluetooth;

    // Currently connected Bluetooth device (null if not connected).
    [ObservableProperty]
    public partial BluetoothDevice? Device { get; set; }
    #endregion

    #region ⚙️ Initialization & Auto-Reconnect
    // --------------------------------------------------------------------
    // Initializes the ViewModel and attempts to restore last Bluetooth session
    // --------------------------------------------------------------------

    /// <summary>
    /// Initializes the ViewModel, optionally auto-reconnecting to the last known device.
    /// </summary>
    ///

    public async Task InitializeAsync() {

        // Request Bluetooth access permission from OS
        if (await bleManager.RequestAccessAsync() != AccessState.Available) {
            await DisplayAlertAsync("Error", "We are trying to connect but bluetooth is not available. Please enable it.", "OK");
            return;
        }

        var deviceId = Preferences.Get(AppConstants.PERIPHERALUUID, string.Empty);
        var connectedPeripheral = !string.IsNullOrEmpty(deviceId) ? bleManager.GetKnownPeripheral(deviceId) : null;

        if (connectedPeripheral is not null && connectedPeripheral.Status == ConnectionState.Connected) {

            await UpdateDeviceConnectionState(ConnectionState.Connected, new BluetoothDevice(connectedPeripheral));

            SubscribeToPeripheral(new BluetoothDevice(connectedPeripheral));
            WatchDeviceForReconnect();
        } else {
            await UpdateDeviceConnectionState(ConnectionState.Disconnected, null);
        }

        // Optional: Auto-reconnect logic here if needed
        if (Device == null && !string.IsNullOrEmpty(deviceId) &&
            Preferences.Get(AppConstants.AUTORECONNECT, false)) {
            var scan = await bleManager.Scan()
                .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(5)))
                .FirstOrDefaultAsync(s => s.Peripheral.Uuid.ToString() == deviceId);

            if (scan?.Peripheral != null) {
                await scan.Peripheral.ConnectAsync(new ConnectionConfig { AutoConnect = false });

                await UpdateDeviceConnectionState(ConnectionState.Connected, new BluetoothDevice(scan.Peripheral));

                WatchDeviceForReconnect();
            }
        }

        var currentUserObj = await UserDb.GetCurrentUser(FirebaseAuthClient.User.Info.Email);

        var prodList = await DatabaseService.GetAllAsync(AppConstants.PRODUCTS_NODE);

        // Grab inventory from all stores the user owns
        var myInventoryList = prodList
            .Where(p => currentUserObj!.StoreIds.Contains(p.StoreId!));

        var existingIds = new HashSet<string>(Products.Select(p => p.Id!));

        foreach (var item in myInventoryList) {
            if (existingIds.Add(item.Id!)) {
                Products.Add(item);
            }
        }

    }


    private void SubscribeToPeripheral(BluetoothDevice device) {

        device.Peripheral.WhenStatusChanged().Subscribe(async status => {
            // Ignore manual disconnect
            if (status == ConnectionState.Disconnected && userInitiatedDisconnect)
                return;

            await UpdateDeviceConnectionState(status, device);
        });

        device.Peripheral.WhenConnectionFailed().Subscribe(async error => {
            await DisplayToastAsync($"Failed to connect to {device.Name}: {error}");
        });
    }

    #endregion

    #region 🔌 Bluetooth Management
    // --------------------------------------------------------------------
    // Handles Bluetooth scanning, connection, and disconnection
    // --------------------------------------------------------------------

    /// <summary>
    /// Disconnects from the currently connected Bluetooth device.
    /// </summary>
    [RelayCommand]
    private async Task DisconnectDevice() {
        var device = Device;

        userInitiatedDisconnect = true; // So we know this was intentional
        Device?.Peripheral.CancelConnection(); // Cancel BLE connection
        await UpdateDeviceConnectionState(ConnectionState.Disconnected, device);
        Housekeeping(); // Stop scanning, cleanup subscriptions
    }


    /// <summary>
    /// Handles Bluetooth access permission and toggles between scanning or showing connected popup.
    /// </summary>
    [RelayCommand]
    private async Task HandleBluetooth() {
        if (_isHandlingBluetooth)
            return; // Prevent re-entry

        _isHandlingBluetooth = true;

        try {
            // Already connected — show popup instead of scanning
            if (Device != null && Device.Peripheral.Status == ConnectionState.Connected) {
                IsDeviceConnectedPopupVisible = true;
            } else {
                // Request Bluetooth access permission from OS
                if (await bleManager.RequestAccessAsync() != AccessState.Available) {
                    await DisplayAlertAsync("Error", "Bluetooth is not available. Please enable it.", "OK");
                    return;
                }

                // Show device selection popup and start scanning
                IsDevicesPopupVisible = true;

                await StartScanningAsync();
            }
        } catch (Exception ex) {
            await DisplayToastAsync($"Bluetooth error: {ex.Message}");
        } finally {
            _isHandlingBluetooth = false;
        }
    }

    /// <summary>
    /// Connects to a selected Bluetooth device from the device list.
    /// </summary>
    [RelayCommand]
    private async Task ConnectToDeviceAsync(BluetoothDevice? device) {
        if (device == null)
            return;

        await DisplayToastAsync($"Connecting to: {device.Name}");

        // Try connecting to the selected peripheral
        await device.Peripheral.ConnectAsync(new ConnectionConfig { AutoConnect = true });

        WatchDeviceForReconnect();

        // Wait until device reports as connected
        await device.Peripheral.WhenConnected().FirstAsync();

        // Update ViewModel/UI state
        await UpdateDeviceConnectionState(ConnectionState.Connected, device);

        // Listen for status changes (e.g. disconnected unexpectedly)
        device.Peripheral.WhenStatusChanged().Subscribe(status => {
            MainThread.BeginInvokeOnMainThread(async () => {
                if (device.Peripheral.Status == ConnectionState.Disconnected && userInitiatedDisconnect)
                    return;

                await UpdateDeviceConnectionState(status, device);
            });
        });

        // Listen for failed connection attempts
        device.Peripheral.WhenConnectionFailed().Subscribe(async error => {
            await DisplayToastAsync($"Failed to connect to {device.Name}, reason: {error}");
        });
    }

    /// <summary>
    /// Starts scanning for nearby BLE devices and populates the device list.
    /// </summary>
    private async Task StartScanningAsync() {

        // Avoid starting multiple scans
        if (bleManager.IsScanning)
            return;

        try {

            Devices.Clear(); // Clear previous scan results

            // Subscribe to device discovery stream
            scanSub = bleManager.Scan().Subscribe(scan => {
                var peripheral = scan.Peripheral;

                if (string.IsNullOrEmpty(peripheral.Name))
                    return; // Skip unnamed devices

                // Add new device if not already in the list
                if (!Devices.Any(p => p.Uuid == peripheral.Uuid)) {
                    var device = new BluetoothDevice(peripheral);
                    MainThread.BeginInvokeOnMainThread(() => Devices.Add(device));
                }
            });
        } catch (Exception ex) {
            await DisplayToastAsync($"Scan failed: {ex.Message}");
        }
    }

    #endregion

    #region 🔄 Connection State Handling
    /// <summary>
    /// Updates the ViewModel/UI based on the current Bluetooth connection state.
    /// </summary>
    private async Task UpdateDeviceConnectionState(ConnectionState state, BluetoothDevice? device) {

        switch (state) {
            case ConnectionState.Disconnected:
                BluetoothIcon = FontsConstants.Bluetooth;
                IsDeviceConnectedPopupVisible = false;

                // Only show toast if there was a connected device before
                if (Device != null || userInitiatedDisconnect) {
                    var reason = userInitiatedDisconnect
                        ? "Disconnected manually"
                        : "Device powered off or out of range";

                    MainThread.BeginInvokeOnMainThread(async () => {
                        await DisplayToastAsync($"Disconnected: {reason}");
                    });
                }

                Housekeeping();
                userInitiatedDisconnect = false;
                Device = null;
                break;

            case ConnectionState.Connected:
                BluetoothIcon = FontsConstants.Bluetooth_connected;
                Device = device;
                await AutoConnect(device!);
                IsDevicesPopupVisible = false;
                break;

            case ConnectionState.Connecting:
                BluetoothIcon = FontsConstants.Bluetooth;
                MainThread.BeginInvokeOnMainThread(async () => {
                    await DisplayToastAsync($"Connecting to {device?.Name}...");
                });
                break;

            case ConnectionState.Disconnecting:
                BluetoothIcon = FontsConstants.Bluetooth;
                MainThread.BeginInvokeOnMainThread(async () => {
                    await DisplayToastAsync("Disconnected");
                });
                break;
        }
    }

    #endregion

    #region 🔒 Auto-Connect Management
    /// <summary>
    /// Asks the user whether to remember this device for auto-reconnect.
    /// </summary>
    private async Task AutoConnect(BluetoothDevice device) {

        var deviceId = Preferences.Get(AppConstants.PERIPHERALUUID, string.Empty);

        if (!string.IsNullOrEmpty(deviceId))
            return; // Already set, no need to ask again

        bool answer = await DisplayConfirmAsync(
            "Auto-Connect",
            "Would you like to enable auto-connect for this device in the future?",
            "Yes",
            "No"
        );

        if (answer) {
            Preferences.Set(AppConstants.PERIPHERALUUID, device.Uuid);
            Preferences.Set(AppConstants.AUTORECONNECT, true);
        } else {
            Preferences.Clear();
        }
    }
    #endregion

    #region 🧹 Cleanup
    // --------------------------------------------------------------------
    // Stops BLE scanning and disposes of active subscriptions
    // --------------------------------------------------------------------

    /// <summary>
    /// Stops BLE scanning and disposes of active subscriptions.
    /// </summary>
    public void Housekeeping() {
        scanSub?.Dispose();
        scanSub = null;
    }
    #endregion

    /// <summary>
    /// Navigates to the NewProductPage to add a new product.
    /// </summary>
    [RelayCommand]
    private async Task AddProduct() {

        await NavigateAsync(nameof(NewProductPage));
    }

    /// <summary>
    /// Closes the device selection popup.
    /// </summary>
    [RelayCommand]
    void CloseDevicesPopUp() {
        IsDevicesPopupVisible = false;
        Housekeeping();
    }

    [RelayCommand]
    async Task PrintAsync(Product product) {
        if (product == null || Device == null)
            return;

        using var http = new HttpClient();
        var pngBytes = await http.GetByteArrayAsync(product.QrUrl);

        await CustomDialogService.ShowAsync("Please Wait", "wait.gif");

        if (!pngBytes.IsNullOrEmpty()) {
            var isFinished = await Printer.PrintQrAsync(Device.Peripheral, pngBytes);

            if (isFinished)
                await CustomDialogService.HideAsync();
        } else {
            // Hide dialog if nothing was fetched
            await CustomDialogService.HideAsync();
        }
    }


    private void WatchDeviceForReconnect() {
        if (Device == null)
            return;

        Device.Peripheral.WhenStatusChanged().Subscribe(async status => {
            await MainThread.InvokeOnMainThreadAsync(async () => {
                if (Device == null)
                    return; // prevent null crash

                // Ignore user-initiated disconnect
                if (status == ConnectionState.Disconnected && userInitiatedDisconnect)
                    return;

                await UpdateDeviceConnectionState(status, Device);

                if (status == ConnectionState.Disconnected) {
                    // Only auto-reconnect if disconnect was NOT triggered by the user
                    if (!userInitiatedDisconnect) {
                        var deviceId = Preferences.Get(AppConstants.PERIPHERALUUID, string.Empty);
                        if (!string.IsNullOrEmpty(deviceId)) {
                            var scan = await bleManager.Scan()
                                .TakeUntil(Observable.Timer(TimeSpan.FromSeconds(5)))
                                .FirstOrDefaultAsync(s => s.Peripheral.Uuid.ToString() == deviceId);

                            if (scan?.Peripheral != null) {
                                await scan.Peripheral.ConnectAsync(new ConnectionConfig { AutoConnect = true });
                                await UpdateDeviceConnectionState(ConnectionState.Connected, new BluetoothDevice(scan.Peripheral));
                                SubscribeToPeripheral(new BluetoothDevice(scan.Peripheral));
                            }
                        }
                    }
                }

            });
        });
    }


}
