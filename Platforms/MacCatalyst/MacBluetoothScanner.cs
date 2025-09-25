namespace SnapLabel.Platforms.MacCatalyst {

    public class MacBluetoothScanner : IBluetoothService {

        public event Action<BluetoothDeviceModel> DeviceFound;

        public void StartScan() {
            throw new NotImplementedException();
        }

        public void StopScan() {
            throw new NotImplementedException();
        }
    }
}
