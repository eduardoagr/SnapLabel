using System.Text;

using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;
using Windows.Devices.Radios;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;

namespace SnapLabel.Platforms.Windows {

    /// <summary>
    /// Windows-specific implementation of IBluetoothService.
    /// Uses DeviceWatcher to discover nearby Bluetooth devices and connects via RFCOMM.
    /// </summary>
    public class WindowsBluetoothScanner : IBluetoothService {

        /// <summary>
        /// Event fired when a new Bluetooth device is found during scanning.
        /// </summary>
        public event Action<BluetoothDeviceModel>? DeviceFound;

        /// <summary>
        /// Event fired when the Bluetooth device connection is lost or disconnected.
        /// </summary>
        public event Action? DeviceDisconnected;
        public event Action<byte[]>? DataReceived;

        private DeviceWatcher? _watcher;                 // Device watcher for scanning
        private bool _keepScanning;                      // Flag for scanning state
        private StreamSocket? _socket;                   // Active RFCOMM socket connection
        private readonly HashSet<string> _seenDevices = []; // Track discovered devices to avoid duplicates

        // Selector string to filter classic Bluetooth (RFCOMM) devices for DeviceWatcher
        private static readonly string ClassicBluetoothSelector =
            "System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\"";

        /// <summary>
        /// Starts scanning for nearby Bluetooth devices.
        /// Clears previously seen devices and starts the DeviceWatcher.
        /// </summary>
        public void StartScan() {
            _keepScanning = true;
            _seenDevices.Clear();
            StartWatcher();
        }

        /// <summary>
        /// Stops scanning for Bluetooth devices.
        /// If the DeviceWatcher is running, it is stopped and disposed.
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
        /// Pairs the device if needed, and establishes StreamSocket connection.
        /// </summary>
        /// <param name="deviceId">The DeviceInformation.Id of the target Bluetooth device.</param>
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
        /// Sends raw byte data over the active RFCOMM connection and waits for acknowledgment.
        /// </summary>
        /// <param name="data">Byte array to send.</param>
        /// <returns>True if the data was sent and an acknowledgment was received, otherwise false.</returns>
        public async Task<bool> SendDataAsync(byte[] data) {
            DataWriter? writer = null;
            DataReader? reader = null;
            try {
                if(_socket == null || _socket.OutputStream == null || _socket.InputStream == null) {
                    Debug.WriteLine("[WARN] SendDataAsync: socket is null or disposed");
                    return false;
                }

                // --- Send Data ---
                writer = new DataWriter(_socket.OutputStream);
                writer.WriteBytes(data);
                await writer.StoreAsync();
                await writer.FlushAsync();
                Debug.WriteLine($"[INFO] Sent {data.Length} bytes: {Encoding.UTF8.GetString(data)}");

                // --- Wait for Acknowledgment ---
                reader = new DataReader(_socket.InputStream);
                reader.InputStreamOptions = InputStreamOptions.Partial;

                var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(8));

                try {
                    // Read available data (flexible size)
                    var bytesRead = await reader.LoadAsync(1024).AsTask(cancellationTokenSource.Token);

                    if(bytesRead >= 2) {
                        string response = reader.ReadString(bytesRead).Trim();
                        Debug.WriteLine($"[INFO] Received acknowledgment: '{response}'");

                        if(response.Contains("OK")) {
                            Debug.WriteLine("[SUCCESS] Data delivery confirmed!");
                            return true;
                        }
                    }
                    else {
                        Debug.WriteLine($"[WARN] Received only {bytesRead} bytes, expected at least 2");
                    }
                } catch(TaskCanceledException) {
                    Debug.WriteLine("[ERROR] Timeout waiting for acknowledgment");
                } catch(Exception readEx) {
                    Debug.WriteLine($"[ERROR] Failed to read acknowledgment: {readEx.Message}");
                }

                return false;

            } catch(Exception ex) {
                Debug.WriteLine($"[ERROR] SendDataAsync failed: {ex.Message}");
                return false;
            } finally {
                // Proper cleanup
                try {
                    writer?.DetachStream();
                    writer?.Dispose();
                } catch { }

                try {
                    reader?.DetachStream();
                    reader?.Dispose();
                } catch { }
            }
        }


        /// <summary>
        /// Disconnects the current RFCOMM connection and unpairs the device if paired.
        /// </summary>
        /// <param name="deviceId">The device identifier string.</param>
        public async Task Disconnect(string deviceId) {
            try {
                if(deviceId is null) {
                    return;
                }

                _socket?.Dispose();
                _socket = null;

                var info = await DeviceInformation.CreateFromIdAsync(deviceId);

                if(info.Pairing.IsPaired) {
                    var result = await info.Pairing.UnpairAsync();
                    Debug.WriteLine($"[INFO] Unpairing result: {result.Status}");
                }

            } catch(Exception ex) {
                Debug.WriteLine($"[ERROR] DisconnectAsync failed: {ex.Message}");
            }
        }

        /// <summary>
        /// Checks if Bluetooth is enabled on the system.
        /// </summary>
        /// <returns>True if Bluetooth radio is available and turned on, otherwise false.</returns>
        public async Task<bool> IsBluetoothEnabledAsync() {
            var radios = await Radio.GetRadiosAsync();
            var bluetoothRadio = radios.FirstOrDefault(r => r.Kind == RadioKind.Bluetooth);
            return bluetoothRadio != null && bluetoothRadio.State == RadioState.On;
        }


        public Task StartListeningAsync() {
            Debug.WriteLine("[Windows] StartListeningAsync called, but not implemented.");
            return Task.CompletedTask;

        }
    }
}
