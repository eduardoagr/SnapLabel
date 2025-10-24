namespace SnapLabel.Models {
    public class BluetoothDevice {

        public IPeripheral Peripheral { get; }

        public string Icon { get; }

        public string Name => Peripheral.Name!;

        public string Uuid => Peripheral.Uuid;

        public BluetoothDevice(IPeripheral peripheral) {

            Peripheral = peripheral;
            Icon = GetIcon(Name);
        }

        public static string GetIcon(string name) {
            name = name?.ToLowerInvariant() ?? "";

            // 🔍 Prefix-based printer detection
            if(name.StartsWith("pp") || name.StartsWith("mtp") || name.StartsWith("pos") || name.StartsWith("bt"))
                return FontsConstants.Print;

            if(name.Contains("tv") || name.Contains("frame") || name.Contains("[av]") ||
                name.Contains("av") || name.Contains("qled") || name.Contains("hdtv") || name.Contains('″'))
                return FontsConstants.Tv;

            if(name.Contains("speaker") || name.Contains("soundbar"))
                return FontsConstants.Speaker;

            if(name.Contains("light") || name.Contains("govee") || name.Contains("ihoment"))
                return FontsConstants.Lightbulb;

            if(name.Contains("printer") || name.StartsWith("wwm") || name.Contains("epson"))
                return FontsConstants.Print;

            if(name.Contains("watch") || name.Contains("wear"))
                return FontsConstants.Watch;

            if(name.Contains("headset") || name.Contains("earbuds") || name.Contains("earbud") || name.Contains("headphones"))
                return FontsConstants.Headphones;

            return FontsConstants.Bluetooth;
        }
    }

}
