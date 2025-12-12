using Supabase;

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
    public static byte[]? CompressImage(byte[] originalBytes, int initialWidth = 1000, int maxSizeKb = 100) {
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


    public static async Task<string?> SupabaseUploadAndGetUrlAsync(IShellService shellService, Client _client, string fileName, byte[] data, string bucket, string extension = "png") {
        if (data is null || data.Length == 0)
            return null;

        var path = $"{fileName}.{extension}";

        var existingObjects = await _client.Storage
            .From(bucket)
            .List();

        if (existingObjects != null && existingObjects.Any(o => o.Name != null && o.Name.Equals(path, StringComparison.OrdinalIgnoreCase))) {

            await shellService.DisplayAlertAsync("Error", "This file aalready exist", "OK");
            return null;
        }

        var uploadResponse = await _client.Storage
            .From(bucket)
            .Upload(data, path);

        if (string.IsNullOrEmpty(uploadResponse))
            return null;

        return $"{AppConstants.SUPABASE_URL}/storage/v1/object/public/{AppConstants.SUPABASE_BUCKET}/{path}";
    }

    public static byte[] GeneerateQR(string data) {

        using var qrGenerator = new QRCodeGenerator();
        using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);

        // Use PngByteQRCode for cross-platform PNG byte[] generation
        var pngQrCode = new PngByteQRCode(qrCodeData);
        byte[] qrBytes = pngQrCode.GetGraphic(40);

        return qrBytes;
    }
}

