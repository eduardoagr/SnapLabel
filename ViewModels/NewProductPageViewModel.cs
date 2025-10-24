namespace SnapLabel.ViewModels {

    public partial class NewProductPageViewModel : ObservableObject {

        #region Readonly and Static Fields
        private static readonly QRCodeGenerator qrGenerator = new();
        private readonly string sharedRoot = @"\\Ed-pc\E\SnapLabel.Images";
        private readonly string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        private readonly IMediaPicker mediaPicker;
        private readonly IShellService shellService;
        //private readonly DatabaseService databaseService;
        #endregion

        public ProductViewModel ProductVM { get; }

        // Explicit command
        public IRelayCommand? SaveProductCommand { get; }

        #region Constructor
        public NewProductPageViewModel(IMediaPicker mediaPicker, IShellService shellService) {
            this.mediaPicker = mediaPicker;
            this.shellService = shellService;
            //this.databaseService = databaseService;


            ProductVM = new ProductViewModel(new Product());
            ProductVM.ProductPropertiesChanged += ProductVM_ProductPropertiesChanged;

            SaveProductCommand = new RelayCommand(async () => await SaveProductAsync(), CanSaveProduct);
        }
        #endregion

        private void ProductVM_ProductPropertiesChanged() {

            SaveProductCommand?.NotifyCanExecuteChanged();
        }

        private bool CanSaveProduct() => ProductVM.CanSave;

        #region Command for picking/capturing images
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

            var compressed = Operations.CompressImage(ms.ToArray());
            ProductVM.ImagePreview = ImageSource.FromStream(() => new MemoryStream(compressed!));
            ProductVM.ImageSize = $"{compressed!.Length / 1024.0:F2} KB";
            ProductVM.ImageBytes = compressed;
            Debug.WriteLine($"[Edu] Compressed image length: {compressed?.Length}");
            ProductVM.SaveToModel();
            Debug.WriteLine($"[Edu] => Model image length: {ProductVM.GetProduct().ImageBytes?.Length}");
        }
        #endregion

        #region Command for saving to database
        public async Task SaveProductAsync() {

            var Product = ProductVM.GetProduct();
            Product.NormalizeName();

            //var productId = await databaseService.TryAddItemAsync(Product);
            //if(productId is null) {

            //    await shellService.DisplayAlertAsync("Duplicate Detected", "A product with the same name already exists.", "OK");
            //    return;
            //}

            //Product.Id = productId.Value;

            // Save image immediately
            string folder = Path.Combine(sharedRoot, $"{Product.Name}_{timestamp}");
            Directory.CreateDirectory(folder);

            string imageFileName = $"{Product.Name}_image.jpg";
            string imageFilePath = Path.Combine(folder, imageFileName);
            File.WriteAllBytes(imageFilePath, Product.ImageBytes!);
            Product.ImagePath = $"file://{imageFilePath}"; // ✅ ensures CachedImage loads instantly

            // Update DB with image path before navigating
            //await databaseService.UpdateItemAsync(Product);

            // Navigate back ASAP
            await shellService.NavigateBackAsync();

            // Optional: QR generation can be deferred if needed
            _ = Task.Run(() => {

                var productJson = JsonSerializer.Serialize(new {
                    Product.Id,
                    Product.Name,
                    Product.Price,
                    Product.ImagePath,
                    Product.Location
                });

                byte[] qrBytes = GenerateQrCodeBytes(productJson);
                string qrFileName = $"{Product.Name}_qr.png";
                string qrFullPath = Path.Combine(folder, qrFileName);
                File.WriteAllBytes(qrFullPath, qrBytes);

                Product.QrCode = qrFullPath;
                Product.GeneratedDate = DateTime.Now.ToString("F");
                Product.IsGenerated = true;

                // await databaseService.UpdateItemAsync(Product);
            });
        }
        #endregion

        #region Method for generating QR code bytes
        public byte[] GenerateQrCodeBytes(string content) {
            var qrCodeData = qrGenerator.CreateQrCode
                (content, QRCodeGenerator.ECCLevel.Q);
            var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(10);
        }
        #endregion
    }
}