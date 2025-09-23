using System.Text.Json;

namespace SnapLabel.ViewModels {

    public partial class NewProductPageViewModel(IMediaPicker mediaPicker,
        IShellService shellService, DatabaseService databaseService) : ObservableObject {

        private static readonly QRCodeGenerator qrGenerator = new();
        private readonly string sharedRoot = @"\\Ed-pc\E\SnapLabel.Images";
        private readonly string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");

        [ObservableProperty]
        public partial Product Product { get; set; } = new();

        [RelayCommand]
        public async Task CaptureImageAsync() {
            if(!mediaPicker.IsCaptureSupported)
                return;

            var photo = await mediaPicker.CapturePhotoAsync();
            if(photo is null)
                return;

            using var stream = await photo.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            var compressed = Operation.CompressImage(ms.ToArray());
            Product.ImagePreview = ImageSource.FromStream(() => new MemoryStream(compressed!));
            Product.ImageSize = $"{compressed!.Length / 1024.0:F2} KB";
            Product.ImageBytes = compressed;
        }

        [RelayCommand]
        public async Task SaveProductAsync() {
            Product.NormalizeName();

            var productId = await databaseService.TryAddItemAsync(Product);
            if(productId is null) {
                await shellService.DisplayAlertAsync("Duplicate Detected", "A product with the same name already exists.", "OK");
                return;
            }

            Product.Id = productId.Value;

            // Save image immediately
            string folder = Path.Combine(sharedRoot, $"{Product.Name}_{timestamp}");
            Directory.CreateDirectory(folder);

            string imageFileName = $"{Product.Name}_image.jpg";
            string imageFilePath = Path.Combine(folder, imageFileName);
            File.WriteAllBytes(imageFilePath, Product.ImageBytes);
            Product.ImagePath = $"file://{imageFilePath}"; // ✅ ensures CachedImage loads instantly

            // Update DB with image path before navigating
            await databaseService.UpdateItemAsync(Product);

            // Navigate back ASAP
            await shellService.NavigateBackAsync();

            // Optional: QR generation can be deferred if needed
            _ = Task.Run(async () => {

                var productJson = JsonSerializer.Serialize(new {
                    Product.Id,
                    Product.Name,
                    Product.Price,
                    Product.ImagePath,
                });

                byte[] qrBytes = GenerateQrCodeBytes(productJson);
                string qrFileName = $"{Product.Name}_qr.png";
                string qrFullPath = Path.Combine(folder, qrFileName);
                File.WriteAllBytes(qrFullPath, qrBytes);

                Product.QrCode = qrFullPath;
                Product.GeneratedDate = DateTime.Now.ToString("F");
                Product.IsGenerated = true;

                await databaseService.UpdateItemAsync(Product);
            });
        }

        public byte[] GenerateQrCodeBytes(string content) {
            var qrCodeData = qrGenerator.CreateQrCode
                (content, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(10);
        }
    }
}