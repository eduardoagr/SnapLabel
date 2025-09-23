namespace SnapLabel.Services {

    public class Operation() {

        public static byte[] CompressImage(byte[] originaBytes, int targetWith = 1000) {

            using var inputStream = new MemoryStream(originaBytes);
            var image = PlatformImage.FromStream(inputStream);

            var resizedImage = image.Downsize(targetWith, true);

            using var outputStream = new MemoryStream();

            float compressionQuality = 0.5f; // 70%
            resizedImage.Save(outputStream, ImageFormat.Jpeg, quality: compressionQuality);

            byte[] compressed = outputStream.ToArray();

            // 🔁 If image is still too big, reduce quality and/or width
            while(compressed.Length > 1024 * 1024 && compressionQuality > 0.3f) {
                compressionQuality -= 0.1f;
                targetWith -= 100;

                resizedImage = image.Downsize(targetWith, true);
                outputStream.SetLength(0); // Clear stream
                resizedImage.Save(outputStream, ImageFormat.Jpeg, quality: compressionQuality);
                compressed = outputStream.ToArray();
            }

            return compressed;

        }

    }
}
