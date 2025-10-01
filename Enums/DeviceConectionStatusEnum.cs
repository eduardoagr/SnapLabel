namespace SnapLabel.Enums {
    public enum DeviceConectionStatusEnum {
        None,
        Connecting,
        Connected,
        Disconnected,
        Failed,
        Printing
    }

    public static class DisconnectReasonEnumExtensions {

        public static string ToDisplayString(this DeviceConectionStatusEnum status) => status switch {
            DeviceConectionStatusEnum.None => "No Device",
            DeviceConectionStatusEnum.Connecting => "Connecting...",
            DeviceConectionStatusEnum.Connected => "Connected",
            DeviceConectionStatusEnum.Disconnected => "Disconnected",
            DeviceConectionStatusEnum.Failed => "Connection Failed",
            DeviceConectionStatusEnum.Printing => "Printing...",
            _ => "Unknown"
        };
    }
}