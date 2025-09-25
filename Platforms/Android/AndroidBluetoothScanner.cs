namespace SnapLabel.Platforms.Android {
    public class AndroidBluetoothScanner : IBluetoothService {

        public event Action<BluetoothDeviceModel> DeviceFound;

        public void StartScan() {
            throw new NotImplementedException();
        }

        public void StopScan() {
            throw new NotImplementedException();
        }
    }
}
