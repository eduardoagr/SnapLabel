namespace SnapLabel.Helpers {

    public class Operations() {

        public static byte[]? CompressImage(byte[] originalBytes, int initialWidth = 1000,
            int maxSizeKb = 20) {
            int targetWidth = initialWidth;
            float compressionQuality = 0.6f;

            byte[] compressed;

            do {
                using var inputStream = new MemoryStream(originalBytes);
                var image = PlatformImage.FromStream(inputStream);

                var resized = image.Downsize(targetWidth, true);

                using var outputStream = new MemoryStream();
                resized.Save(outputStream, ImageFormat.Jpeg, quality: compressionQuality);
                compressed = outputStream.ToArray();

                if(compressed.Length <= maxSizeKb * 1024)
                    return compressed;

                compressionQuality -= 0.1f;
                targetWidth -= 100;

            } while(compressionQuality >= 0.3f && targetWidth >= 300);

            return null;
        }

        public static void SavePreferenceInJson(IPreferences preferences,
            BluetoothDeviceModel bluetoothDeviceModel) {

            if(bluetoothDeviceModel is not null) {
                var deviceJson = JsonSerializer.Serialize(bluetoothDeviceModel);
                preferences.Set(Constants.Constants.LastConnectedDevice, deviceJson);
            }
        }

        public static BluetoothDeviceModel LoadDeviceFromPreferences(IPreferences preferences) {

            if(preferences is not null) {

                var json = preferences.Get(Constants.Constants.LastConnectedDevice,
                    string.Empty);

                if(!string.IsNullOrWhiteSpace(json)) {
                    var device = JsonSerializer.Deserialize<BluetoothDeviceModel>(json);
                    return device!;
                }
            }

            return null!;
        }

        public static bool IsDeviceAlreadySaved(IPreferences preferences, BluetoothDeviceModel deviceModel) {

            var savedDevice = LoadDeviceFromPreferences(preferences);

            return savedDevice is not null && savedDevice.DeviceId == deviceModel.DeviceId;
        }
    }
}