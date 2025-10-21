namespace SnapLabel.Models {

    [Table("Products")]
    public class Product {

        [PrimaryKey, AutoIncrement]
        [Column("Id")]
        public long Id { get; set; }

        [Column("Name")]
        public string? Name { get; set; }

        [Column("Price")]
        public decimal? Price { get; set; }

        [Column("ImagePath")]
        public string? ImagePath { get; set; }

        [Column("QR")]
        public string? QrCode { get; set; }

        [Column("Size")]
        public string? ImageSize { get; set; }

        [Column("IsGenerated")]
        public bool IsGenerated { get; set; }

        [Column("Generated date")]
        public string? GeneratedDate { get; set; }

        [Column("Location")]
        public string? Location { get; set; }

        [Ignore]
        public ImageSource? ImagePreview { get; set; }

        public byte[]? ImageBytes { get; set; }

        public void NormalizeName() {
            Name = Normalize.NormalizeStrings(Name);
        }
    }

}
