namespace SnapLabel.Models {

    [Table("Products")]
    public partial class Product : ObservableObject {

        [PrimaryKey, AutoIncrement]
        [ObservableProperty]
        [Column("Id")]
        public partial long Id { get; set; }


        [ObservableProperty]
        [Column("Name")]
        public partial string Name { get; set; }


        [ObservableProperty]
        [Column("Price")]
        public partial decimal Price { get; set; }


        [ObservableProperty]
        [Column("ImagePath")]
        public partial string ImagePath { get; set; }

        [ObservableProperty]
        [Column("QR")]
        public partial string QrCode { get; set; }

        [ObservableProperty]
        [Column("Size")]
        public partial string ImageSize { get; set; }

        [ObservableProperty]
        [Column("IsGenerated")]
        public partial bool IsGenerated { get; set; }

        [ObservableProperty]
        [Column("Generated date")]
        public partial string GeneratedDate { get; set; }

        [ObservableProperty]
        [Ignore]
        public partial ImageSource? ImagePreview { get; set; }

        [ObservableProperty]
        [Ignore]
        public partial string IsImageLoaded { get; set; }

        [ObservableProperty]
        [Ignore]
        public partial byte[] ImageBytes { get; set; }

        public void NormalizeName() {
            Name = Normalize.NormalizeStrings(Name);
        }
    }
}
