using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SnapLabel.Models {
    public class BluetoothDeviceMessage(BluetoothDeviceModel device) : ValueChangedMessage<BluetoothDeviceModel>(device) {
    }
}
