using CommunityToolkit.Mvvm.Messaging.Messages;

namespace SnapLabel.Models {
    public class PrintCompletedMessage(bool value) : ValueChangedMessage<bool>(value) { }
}
