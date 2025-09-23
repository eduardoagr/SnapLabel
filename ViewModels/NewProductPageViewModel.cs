using System.Text.Json;

namespace SnapLabel.ViewModels {

    public partial class NewProductPageViewModel(IMediaPicker mediaPicker,
        IShellService shellService, DatabaseService databaseService) : ObservableObject {

        private static readonly QRCodeGenerator qrGenerator = new();

        private readonly string sharedRoot = @"\\Ed-pc\E\SnapLabel.Images";


        [ObservableProperty]
        public partial Product Product { get; set; } = new();

        [RelayCommand]
        public async Task CaptureImageAsync() {

            if(!mediaPicker.IsCaptureSupported) {
                return;
            }

            var photo = await mediaPicker.CapturePhotoAsync();
            if(photo is null) {
                return;
            }

            using var stream = await photo.OpenReadAsync();
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);

            var compressed = Operation.CompressImage(ms.ToArray());
            Product.ImagePreview = ImageSource.FromStream(() => new MemoryStream(compressed));
            Product.ImageSize = $"{compressed.Length / 1024.0:F2} KB";

            /* Save image to external drive
            
            // Note: this will only work on Windows, after ensuring this works well, we should consider,
            // saving to OneDrive or another cloud storage for cross-platform access.
            */
            Product.NormalizeName();

            Product.ImagePath = SaveImage(compressed, Product.Name);
        }

        [RelayCommand]
        async Task SaveProductAsync() {

            Product.NormalizeName();

            var productJson = JsonSerializer.Serialize(new {
                Product.Id,
                Product.Name,
                Product.ImagePath,
            });

            // Generate QR code from JSON
            byte[] qrBytes = GenerateQrCodeBytes(productJson);
            string qrFileName = $"{Product.Name}.png";

            var productFolder = Path.GetDirectoryName(Product.ImagePath);
            string qrFullPath = Path.Combine(productFolder!, qrFileName);

            File.WriteAllBytes(qrFullPath, qrBytes);
            Product.QrCode = qrFullPath;
            Product.GeneratedDate = DateTime.Now.ToString("F");
            Product.IsGenerated = true;

            var added = await databaseService.TryAddItemAsync(Product);

            string title = added ? "Success" : "Duplicate Detected";

            string message = added ? "The product was added successfully." :
                "A product with the same ID, name, already exists.";

            await shellService.DisplayAlertAsync(title, message, "OK");

            if(added) {
                await shellService.NavigateBackAsync();
            }
        }


        public byte[] GenerateQrCodeBytes(string content) {

            var qrCodeData = qrGenerator.CreateQrCode(
                content, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(20);
        }

        private string SaveImage(byte[] imageBytes, string productName) {

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string folder = Path.Combine(sharedRoot, $"{productName}_{timestamp}");
            Directory.CreateDirectory(folder);

            string imagePath = Path.Combine(folder, $"{productName}.jpg");
            File.WriteAllBytes(imagePath, imageBytes);
            return imagePath;
        }
    }
}