using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace SnapLabel.Platforms.Windows {

    /// <summary>
    /// Windows-specific implementation of IBluetoothService.
    /// Uses DeviceWatcher to discover nearby Bluetooth devices and connects via RFCOMM.
    /// </summary>
    public class WindowsBluetoothScanner : IBluetoothService {

        // Event fired when a new Bluetooth device is found during scanning
        public event Action<BluetoothDeviceModel>? DeviceFound;

        // Event fired when the Bluetooth device connection is lost or disconnected
        public event Action? DeviceDisconnected;

        private DeviceWatcher? _watcher;                 // Device watcher for scanning
        private CancellationTokenSource? _monitorTokenSource; // For connection health monitoring
        private bool _keepScanning;                       // Flag for scanning state
        private StreamSocket? _socket;                     // Active RFCOMM socket connection
        private readonly HashSet<string> _seenDevices = []; // Track discovered devices to avoid duplicates

        // Selector string to filter classic Bluetooth (RFCOMM) devices for DeviceWatcher
        private static readonly string ClassicBluetoothSelector =
            "System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\"";

        /// <summary>
        /// Starts scanning for nearby Bluetooth devices.
        /// Clears previously seen devices and starts DeviceWatcher.
        /// </summary>
        public void StartScan() {
            _keepScanning = true;
            _seenDevices.Clear();
            StartWatcher();
        }

        /// <summary>
        /// Stops scanning and shuts down the DeviceWatcher if running.
        /// </summary>
        public void StopScan() {
            _keepScanning = false;

            if(_watcher != null &&
                (_watcher.Status == DeviceWatcherStatus.Started ||
                 _watcher.Status == DeviceWatcherStatus.EnumerationCompleted)) {
                _watcher.Stop();
                _watcher = null;
            }
        }

        /// <summary>
        /// Creates and starts the DeviceWatcher to listen for classic Bluetooth devices.
        /// Handles Added, Updated, Removed, and EnumerationCompleted events.
        /// </summary>
        private void StartWatcher() {
            _watcher = DeviceInformation.CreateWatcher(
                ClassicBluetoothSelector,
                ["System.Devices.Aep.DeviceAddress", "System.ItemNameDisplay"],
                DeviceInformationKind.AssociationEndpoint
            );

            // Called when a new Bluetooth device is detected
            _watcher.Added += async (s, deviceInfo) => {
                // Determine device name or fallback to address
                string name = !string.IsNullOrWhiteSpace(deviceInfo.Name)
                    ? deviceInfo.Name
                    : deviceInfo.Properties.TryGetValue("System.Devices.Aep.DeviceAddress", out var addr)
                        ? addr?.ToString() ?? "Unknown"
                        : "Unknown";

                // Clean up device name if it ends with "(Bluetooth)"
                if(name.EndsWith("(Bluetooth)", StringComparison.OrdinalIgnoreCase))
                    name = name.Replace("(Bluetooth)", "").Trim();

                // Avoid duplicate devices
                if(!_seenDevices.Add(name))
                    return;

                string fontIcon = FontsConstants.Bluetooth;

                // Try to retrieve the device's major class to assign an icon
                try {
                    var btDevice = await BluetoothDevice.FromIdAsync(deviceInfo.Id);
                    if(btDevice != null) {
                        uint major = (uint)btDevice.ClassOfDevice.MajorClass;
                        fontIcon = major switch {
                            1 => FontsConstants.Computer,
                            2 => FontsConstants.Smartphone,
                            4 => FontsConstants.Headphones,
                            5 => FontsConstants.Mouse,
                            6 => FontsConstants.Print,
                            7 => FontsConstants.Keyboard,
                            _ => FontsConstants.Bluetooth_audio
                        };
                    }
                } catch(Exception ex) {
                    Debug.WriteLine($"[WARN] Could not get ClassOfDevice for {name}: {ex.Message}");
                }

                // Notify subscribers that a device was found
                DeviceFound?.Invoke(new BluetoothDeviceModel {
                    Name = name,
                    FontIcon = fontIcon,
                    DeviceId = deviceInfo.Id,
                    Address = deviceInfo.Properties["System.Devices.Aep.DeviceAddress"]
                        .ToString()?.Replace(":", "")?.ToUpperInvariant() ?? "000000000000"
                });
            };

            // Called when a device's information is updated
            _watcher.Updated += (s, update) => { };

            // Called when a device is removed (out of range or turned off)
            _watcher.Removed += (s, update) => { };

            // Called when the initial scan/enumeration completes
            _watcher.EnumerationCompleted += (s, e) => { };

            _watcher.Start();
        }

        /// <summary>
        /// Attempts to connect to the Bluetooth device via RFCOMM Serial Port Profile.
        /// Pairs device if needed, and establishes StreamSocket connection.
        /// Also starts a background monitor to detect disconnects.
        /// </summary>
        /// <param name="deviceId">Device identifier string to connect.</param>
        /// <returns>True if connection succeeded, otherwise false.</returns>
        public async Task<bool> ConnectAsync(string deviceId) {
            try {
                if(string.IsNullOrWhiteSpace(deviceId))
                    return false;

                var info = await DeviceInformation.CreateFromIdAsync(deviceId);
                if(info == null)
                    return false;

                // Pair device if it is not paired yet and can be paired
                if(info.Pairing.CanPair && !info.Pairing.IsPaired) {
                    var result = await info.Pairing.PairAsync();
                    if(result.Status != DevicePairingResultStatus.Paired)
                        return false;
                }

                var device = await BluetoothDevice.FromIdAsync(deviceId);
                if(device == null)
                    return false;

                // Get RFCOMM services and pick the first available
                var services = await device.GetRfcommServicesAsync();
                var service = services.Services.FirstOrDefault();
                if(service == null)
                    return false;

                // Initialize StreamSocket and connect
                _socket = new StreamSocket();

                await _socket.ConnectAsync(service.ConnectionHostName, service.ConnectionServiceName);

                return true;
            } catch(Exception ex) {
                Debug.WriteLine($"[ERROR] ConnectAsync failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Starts a background task that periodically sends a harmless ping to check connection health.
        /// If a send operation fails, assumes the connection was lost and triggers disconnect notification.
        /// </summary>
        private void StartConnectionMonitor() {
            _monitorTokenSource?.Cancel(); // Cancel any existing monitor task
            _monitorTokenSource = new CancellationTokenSource();
            var token = _monitorTokenSource.Token;

            Task.Run(async () => {
                while(!token.IsCancellationRequested) {
                    try {
                        if(_socket == null)
                            break;

                        using(var writer = new DataWriter(_socket.OutputStream)) {
                            writer.WriteBytes([0x00]); // harmless ping byte
                            await writer.StoreAsync().AsTask(token);
                        }

                        await Task.Delay(3000, token); // Wait 3 seconds between pings
                    } catch(Exception ex) {
                        Debug.WriteLine($"[WARN] Disconnected during monitor: {ex.Message}");
                        NotifyDisconnected(); // Cleanup and notify listeners
                        break;
                    }
                }
            }, token);
        }

        /// <summary>
        /// Sends raw byte data over the active RFCOMM connection.
        /// </summary>
        /// <param name="data">Data bytes to send.</param>
        /// <returns>True if data was sent successfully, otherwise false.</returns>
        public async Task<bool> SendDataAsync(byte[] data) {
            try {
                if(_socket == null)
                    return false;

                using var writer = new DataWriter(_socket.OutputStream);
                writer.WriteBytes(data);
                await writer.StoreAsync();
                await writer.FlushAsync();
                return true;
            } catch(Exception ex) {
                Debug.WriteLine($"[ERROR] SendDataAsync failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Disconnects the current RFCOMM connection and stops monitoring.
        /// Notifies subscribers via <see cref="DeviceDisconnected"/> event.
        /// </summary>
        public void Disconnect() {
            NotifyDisconnected();
        }

        /// <summary>
        /// Cleans up socket and monitoring resources, and fires the DeviceDisconnected event.
        /// </summary>
        private void NotifyDisconnected() {
            try {
                _monitorTokenSource?.Cancel();
                _monitorTokenSource?.Dispose();
                _monitorTokenSource = null;

                _socket?.Dispose();
                _socket = null;

                DeviceDisconnected?.Invoke();
            } catch(Exception ex) {
                Debug.WriteLine($"[ERROR] NotifyDisconnected failed: {ex.Message}");
            }
        }
    }
}
