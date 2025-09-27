
using CoreBluetooth;

using CoreFoundation;

using Foundation;

namespace SnapLabel.Platforms.iOS {

    public class AppleBluetoothScanner : IBluetoothService {

        public event Action<BluetoothDeviceModel>? DeviceFound;

        private CBCentralManager? _centralManager;
        private readonly HashSet<string> _seenDevices = [];
        private bool _keepScanning;

        public void StartScan() {

            _keepScanning = true;
            _seenDevices.Clear();

            if(_centralManager == null) {
                _centralManager = new CBCentralManager(new CentralDelegate(this), DispatchQueue.MainQueue);
            }
            else if(_centralManager.State == CBManagerState.PoweredOn) {
                _centralManager.ScanForPeripherals(peripheralUuids: null);
            }
        }

        public void StopScan() {
            _keepScanning = false;

            if(_centralManager?.IsScanning == true) {
                _centralManager.StopScan();
            }
        }

        private class CentralDelegate(AppleBluetoothScanner scanner) : CBCentralManagerDelegate {

            public override void UpdatedState(CBCentralManager central) {
                if(central.State == CBManagerState.PoweredOn && scanner._keepScanning) {
                    central.ScanForPeripherals(peripheralUuids: null);
                }
            }

            public override void DiscoveredPeripheral(CBCentralManager central, CBPeripheral peripheral, NSDictionary advertisementData, NSNumber rssi) {
                var name = peripheral.Name ?? peripheral.Identifier.ToString();
                if(!scanner._seenDevices.Add(name))
                    return;

                var model = new BluetoothDeviceModel {
                    Name = name,
                    Address = peripheral.Identifier.ToString(),
                    FontIcon = FontsConstants.Bluetooth
                };

                scanner.DeviceFound?.Invoke(model);
            }
        }
    }
}
