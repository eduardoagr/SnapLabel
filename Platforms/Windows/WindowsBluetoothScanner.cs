using Windows.Devices.Bluetooth;
using Windows.Devices.Enumeration;

namespace SnapLabel.Platforms.Windows {

    /// <summary>
    /// Windows-specific implementation of IBluetoothService.
    /// Uses DeviceWatcher to discover nearby Bluetooth devices.
    /// </summary>
    public class WindowsBluetoothScanner : IBluetoothService {

        public event Action<BluetoothDeviceModel>? DeviceFound;

        private DeviceWatcher? _watcher;
        private bool _keepScanning;
        private readonly HashSet<string> _seenDevices = [];


        public void StartScan() {

            _keepScanning = true;
            _seenDevices.Clear();
            StartWatcher();
        }

        public void StopScan() {
            _keepScanning = false;

            if(_watcher != null &&
                (_watcher.Status == DeviceWatcherStatus.Started ||
                 _watcher.Status == DeviceWatcherStatus.EnumerationCompleted)) {
                _watcher.Stop();
                _watcher = null;
            }
        }

        private void StartWatcher() {
            string selector = "System.Devices.Aep.ProtocolId:=\"{e0cbf06c-cd8b-4647-bb8a-263b43f0f974}\"";

            _watcher = DeviceInformation.CreateWatcher(
                selector,
                [
                    "System.Devices.Aep.DeviceAddress",
                    "System.ItemNameDisplay"
                ],
                DeviceInformationKind.AssociationEndpoint
            );

            _watcher.Added += async (s, deviceInfo) => {
                string name = !string.IsNullOrWhiteSpace(deviceInfo.Name)
                    ? deviceInfo.Name
                    : deviceInfo.Properties.TryGetValue("System.Devices.Aep.DeviceAddress", out var addr)
                        ? addr?.ToString() ?? "Unknown"
                        : "Unknown";

                if(name.EndsWith("(Bluetooth)", StringComparison.OrdinalIgnoreCase))
                    name = name.Replace("(Bluetooth)", "").Trim();

                if(!_seenDevices.Add(name))
                    return;

                string fontIcon = FontsConstants.Bluetooth;

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

                DeviceFound?.Invoke(new BluetoothDeviceModel {
                    Name = name,
                    Address = deviceInfo.Properties.TryGetValue("System.Devices.Aep.DeviceAddress", out var a)
                        ? a?.ToString() ?? ""
                        : "",
                    FontIcon = fontIcon
                });
            };

            _watcher.EnumerationCompleted += (s, e) => {
                Debug.WriteLine("[INFO] Enumeration completed. Watching for new devices...");
            };

            _watcher.Updated += (s, update) => {
                // Optional: handle property changes
            };

            _watcher.Removed += (s, update) => {
                // Optional: handle device removal
            };

            _watcher.Start();
        }
    }
}