namespace SnapLabel.Interfaces;

public interface IBluetoothService {

    /// <summary>
    /// Fired every time a new device is found
    /// </summary>

    event Action<BluetoothDeviceModel> DeviceFound;

    /// <summary>
    /// Start scanning for nearby devices
    /// </summary>
    void StartScan();

    /// <summary>
    /// Stop scanning
    /// </summary>
    void StopScan();

}
