using CommunityToolkit.Mvvm.Messaging;

namespace SnapLabel.Models;

public partial class BluetoothDeviceModel : ObservableObject {

    public string? Name { get; set; }

    public string? Address { get; set; }

    public string FontIcon { get; set; } = FontsConstants.Bluetooth;

    public string DeviceId { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string Status { get; set; }

    partial void OnStatusChanged(string value) {
        WeakReferenceMessenger.Default.Send(new BluetoothDeviceMessage(this));

        Debug.WriteLineIf(value == DeviceConectionStatusEnum.Disconnected.ToDisplayString(), "We disconnect");
    }
}