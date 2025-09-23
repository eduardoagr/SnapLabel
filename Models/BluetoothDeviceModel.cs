namespace SnapLabel.Models;

public class BluetoothDeviceModel {

    public required string Name { get; set; }

    public required string Address { get; set; }

    public required string FontIcon { get; set; } = FontsConstants.Bluetooth;
}
