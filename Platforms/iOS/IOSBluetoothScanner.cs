namespace SnapLabel.Platforms.iOS {

    public class IOSBluetoothScanner : IBluetoothService {

        public event Action<BluetoothDeviceModel> DeviceFound;

        public void StartScan() {
            throw new NotImplementedException();
        }

        public void StopScan() {
            throw new NotImplementedException();
        }
    }
}
