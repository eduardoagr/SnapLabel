using Android.Bluetooth;
using Android.Content;

using System.Text;

namespace SnapLabel.Platforms.Android {
    public class AndroidBluetoothScanner : IBluetoothService {
        readonly IShellService? shellService;
        public event Action<BluetoothDeviceModel>? DeviceFound;
        public event Action? DeviceDisconnected;
        public event Action<byte[]>? DataReceived;

        private readonly BluetoothAdapter? _bluetoothAdapter;
        private readonly HashSet<string> _seenDevices = [];
        private BluetoothSocket? _socket;
        private bool _keepScanning;
        private readonly BroadcastReceiver? _receiver;

        public AndroidBluetoothScanner(IShellService shell) {

            if(BluetoothPermissionHelper.EnsureBluetoothScanPermission() == true) {
                shellService = shell;
                _bluetoothAdapter = BluetoothAdapter.DefaultAdapter
                    ?? throw new InvalidOperationException("No Bluetooth adapter found on this device.");

                _receiver = new BluetoothScanReceiver(OnDeviceFound);
            }
        }

        public async Task StartListeningAsync() {
            var sppUuid = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
            var serverSocket = BluetoothAdapter.DefaultAdapter.ListenUsingRfcommWithServiceRecord("SnapLabel", sppUuid);
            ShowToast("📡 Listening for Bluetooth...");


            while(true) {
                try {
                    var socket = await serverSocket.AcceptAsync();
                    ShowToast("🔗 Connected to PC");

                    var buffer = new byte[1024];
                    int bytesRead = await socket.InputStream.ReadAsync(buffer, 0, buffer.Length);
                    string received = Encoding.UTF8.GetString(buffer, 0, bytesRead);
                    ShowToast($"📥 Received: {received}");

                    var echo = Encoding.UTF8.GetBytes("OK");
                    await socket.OutputStream.WriteAsync(echo, 0, echo.Length);
                    await socket.OutputStream.FlushAsync();
                    ShowToast("📤 Echo sent: OK");
                } catch(Exception ex) {

                    ShowToast($"❌ Error: {ex.Message}");
                }
            }
        }

        private void ShowToast(string message) {
            shellService?.DisplayToastAsync(message, ToastDuration.Short);
        }

        public void StartScan() {
            if(!_bluetoothAdapter!.IsEnabled) {
                System.Diagnostics.Debug.WriteLine("[INFO] Bluetooth is disabled.");
                return;
            }

            _keepScanning = true;
            _seenDevices.Clear();

            Platform.AppContext.RegisterReceiver(_receiver, new IntentFilter(BluetoothDevice.ActionFound));
            _bluetoothAdapter.StartDiscovery();
            System.Diagnostics.Debug.WriteLine("[INFO] Started Bluetooth discovery.");
        }

        public void StopScan() {
            _keepScanning = false;

            if(_bluetoothAdapter.IsDiscovering)
                _bluetoothAdapter.CancelDiscovery();

            try {
                Platform.AppContext.UnregisterReceiver(_receiver);
            } catch { }

            System.Diagnostics.Debug.WriteLine("[INFO] Stopped Bluetooth discovery.");
        }

        public async Task<bool> ConnectAsync(string deviceId) {
            try {
                var device = _bluetoothAdapter.GetRemoteDevice(deviceId);
                if(device == null)
                    return false;

                var sppUuid = Java.Util.UUID.FromString("00001101-0000-1000-8000-00805F9B34FB");
                _socket = device.CreateRfcommSocketToServiceRecord(sppUuid);

                if(_bluetoothAdapter.IsDiscovering)
                    _bluetoothAdapter.CancelDiscovery();

                await _socket!.ConnectAsync();
                System.Diagnostics.Debug.WriteLine($"[INFO] Connected to {device.Name}.");
                return true;
            } catch(Exception ex) {
                System.Diagnostics.Debug.WriteLine($"[ERROR] ConnectAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task<bool> SendDataAsync(byte[] data) {
            try {
                if(_socket == null || !_socket.IsConnected)
                    return false;

                await _socket.OutputStream!.WriteAsync(data, 0, data.Length);
                await _socket.OutputStream.FlushAsync();

                System.Diagnostics.Debug.WriteLine("[INFO] Data sent.");
                return true;
            } catch(Exception ex) {
                System.Diagnostics.Debug.WriteLine($"[ERROR] SendDataAsync failed: {ex.Message}");
                return false;
            }
        }

        public async Task Disconnect(string deviceId) {
            try {
                _socket?.Close();
                _socket = null;
                System.Diagnostics.Debug.WriteLine("[INFO] Disconnected.");
            } catch(Exception ex) {
                System.Diagnostics.Debug.WriteLine($"[ERROR] Disconnect failed: {ex.Message}");
            }

            await Task.CompletedTask;
        }

        public Task<bool> IsBluetoothEnabledAsync() {
            return Task.FromResult(_bluetoothAdapter?.IsEnabled ?? false);
        }

        private void OnDeviceFound(BluetoothDevice device) {
            if(device == null || string.IsNullOrWhiteSpace(device.Address))
                return;

            if(!_seenDevices.Add(device.Address))
                return;

            string fontIcon = FontsConstants.Bluetooth;

            try {
                var btClass = device.BluetoothClass;
                int major = (int)(btClass?.MajorDeviceClass ?? 0);

                fontIcon = major switch {
                    256 => FontsConstants.Computer,
                    512 => FontsConstants.Smartphone,
                    1024 => FontsConstants.Headphones,
                    1280 => FontsConstants.Mouse,
                    1536 => FontsConstants.Print,
                    1792 => FontsConstants.Watch,
                    7936 => FontsConstants.Bluetooth_audio,
                    _ => FontsConstants.Bluetooth
                };
            } catch(Exception ex) {
                System.Diagnostics.Debug.WriteLine($"[WARN] Could not map ClassOfDevice: {ex.Message}");
            }

            DeviceFound?.Invoke(new BluetoothDeviceModel {
                Name = device.Name ?? "Unknown",
                FontIcon = fontIcon,
                DeviceId = device.Address,
                Address = device.Address.Replace(":", "").ToUpperInvariant()
            });
        }
    }

    internal class BluetoothScanReceiver(Action<BluetoothDevice> onDeviceFound) : BroadcastReceiver {
        public override void OnReceive(Context? context, Intent? intent) {
            if(intent?.Action == BluetoothDevice.ActionFound) {
                var device = (BluetoothDevice?)intent.GetParcelableExtra(BluetoothDevice.ExtraDevice);
                if(device != null)
                    onDeviceFound(device);
            }
        }
    }
}