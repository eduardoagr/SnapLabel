using Android.Bluetooth;
using Android.Content;
using Android.Util;

namespace SnapLabel.Platforms.Android {
    public class AndroidBluetoothScanner : IBluetoothService {

        public event Action<BluetoothDeviceModel>? DeviceFound;
        public event Action DeviceDisconnected;

        private readonly BluetoothAdapter _bluetoothAdapter = BluetoothAdapter.DefaultAdapter;
        private readonly HashSet<string> _seenDevices = [];
        private BluetoothScanReceiver? _receiver;
        private bool _keepScanning;

        public void StartScan() {

            if(_bluetoothAdapter is null) {
                return;
            }

            if(!_bluetoothAdapter.IsEnabled) {
                return;
            }

            if(!BluetoothPermissionHelper.EnsureBluetoothScanPermission()) {
                return;
            }

            _keepScanning = true;
            _seenDevices.Clear();

            _receiver = new BluetoothScanReceiver(
                onDeviceFound: device => {
                    if(_seenDevices.Add(device.Address))
                        DeviceFound?.Invoke(device);
                },
                onDiscoveryFinished: RestartDiscoveryIfNeeded
            );

            var filter = new IntentFilter();
            filter.AddAction(BluetoothDevice.ActionFound);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryStarted);
            filter.AddAction(BluetoothAdapter.ActionDiscoveryFinished);

            Platform.AppContext.RegisterReceiver(_receiver, filter);


            bool started = false;
            try {
                started = _bluetoothAdapter.StartDiscovery();
            } catch(Exception ex) {
                Debug.WriteLine($"StartDiscovery threw: {ex}");
            }
        }

        public void StopScan() {

            _keepScanning = false;

            try {
                if(_bluetoothAdapter?.IsDiscovering == true) {
                    _bluetoothAdapter.CancelDiscovery();
                }
            } catch(Exception ex) {
                Debug.WriteLine($"CancelDiscovery threw: {ex}");
            }

            if(_receiver != null) {
                try {
                    Platform.AppContext.UnregisterReceiver(_receiver);
                } catch(Exception ex) {
                    Debug.WriteLine($"UnregisterReceiver threw: {ex}");
                } finally {
                    _receiver = null;
                }
            }
        }

        private void RestartDiscoveryIfNeeded() {

            if(!_keepScanning)
                return;

            // small delay before restarting to avoid hammering the stack
            Task.Run(async () => {
                await Task.Delay(1000);
                if(!_keepScanning)
                    return;

                try {
                    bool started = _bluetoothAdapter.StartDiscovery();
                } catch(Exception ex) {
                    Debug.WriteLine($"Restart StartDiscovery threw: {ex}");

                }
            });
        }

        public Task<bool> ConnectAsync(string address) {
            throw new NotImplementedException();
        }

        public Task<bool> SendDataAsync(byte[] data) {
            throw new NotImplementedException();
        }

        public void Disconnect() {
            throw new NotImplementedException();
        }

        private class BluetoothScanReceiver(Action<BluetoothDeviceModel> onDeviceFound, Action onDiscoveryFinished) : BroadcastReceiver {

            public override void OnReceive(Context context, Intent intent) {
                var action = intent?.Action ?? "<null>";
                switch(action) {
                    case BluetoothAdapter.ActionDiscoveryStarted:
                        break;

                    case BluetoothAdapter.ActionDiscoveryFinished:
                        onDiscoveryFinished?.Invoke();
                        break;

                    case BluetoothDevice.ActionFound:
                        var device = (BluetoothDevice?)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                        if(device == null) {
                            return;
                        }

                        var name = !string.IsNullOrWhiteSpace(device.Name) ? device.Name : device.Address;
                        int major = device.BluetoothClass != null ? (int)device.BluetoothClass.MajorDeviceClass : -1;

                        var model = new BluetoothDeviceModel {
                            Name = name ?? string.Empty,
                            Address = device.Address ?? string.Empty,
                            FontIcon = GetFontIcon(device),
                            DeviceId = device.Address ?? string.Empty
                        };

                        Log.Info("BluetoothScan", $"Device: {model.Name}, MajorClass: {major}, Icon: {model.FontIcon}");



                        onDeviceFound?.Invoke(model);


                        break;

                    default:

                        break;
                }
            }

            private static string GetFontIcon(BluetoothDevice device) {
                var btClass = device.BluetoothClass;
                if(btClass == null)
                    return FontsConstants.Notification_important; // fallback

                int major = (int)btClass.MajorDeviceClass;

                return major switch {
                    256 => FontsConstants.Computer,
                    512 => FontsConstants.Smartphone,
                    1024 => FontsConstants.Headphones,
                    1280 => FontsConstants.Mouse,
                    1536 => FontsConstants.Print,
                    1792 => FontsConstants.Watch,
                    7936 => FontsConstants.Bluetooth_audio, // Uncategorized
                    _ => FontsConstants.Bluetooth // fallback
                };
            }
        }
    }
}

