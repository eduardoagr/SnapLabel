namespace SnapLabel.Helpers;

/// <summary>
/// Provides utility methods for image compression, QR code generation, and cloud storage operations.
/// </summary>
public class Operations {
    /// <summary>
    /// Compresses an image by resizing and reducing quality until it meets the specified maximum size.
    /// </summary>
    /// <param name="originalBytes">The original image as a byte array.</param>
    /// <param name="initialWidth">The starting width for resizing. Default is 1000 pixels.</param>
    /// <param name="maxSizeKb">The maximum allowed size in kilobytes. Default is 5 KB.</param>
    /// <returns>The compressed image as a byte array, or null if compression fails to meet the size constraint.</returns>
    public static byte[]? CompressImage(byte[] originalBytes, int initialWidth = 1000, int maxSizeKb = 10) {
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
        }
        while(compressionQuality >= 0.1f && targetWidth >= 100);

        return null;
    }


    /// <summary>
    /// Generates a QR code from a serialized product object.
    /// </summary>
    /// <param name="product">The product object containing details to encode.</param>
    /// <returns>A PNG image of the QR code as a byte array.</returns>
    //public static byte[] GenerateProdutQrCode(Product product) {
    //    var Obj = new ProductDto {
    //        Id = product.Id,
    //        Name = product.Name,
    //        Price = product.Price,
    //        Location = product.Location,
    //        ImageBytes = product.ImageBytes,
    //        GeneratedDate = product.GeneratedDate,
    //    };

    //    var json = JsonSerializer.Serialize(Obj);

    //    return GenerateQrCode(json);



}

