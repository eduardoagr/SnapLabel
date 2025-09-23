namespace SnapLabel.Services {

    public class Operation() {

        public static byte[]? CompressImage(byte[] originalBytes, int initialWidth = 1000, int maxSizeKb = 20) {
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
    }
}