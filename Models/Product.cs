using Supabase.Postgrest.Attributes;

namespace SnapLabel.Models;
[Table("products")]
public class Product {

    [PrimaryKey]
    [Column("id")]
    public long Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("price")]
    public decimal? Price { get; set; }

    [Column("imagepath")]
    public string? ImagePath { get; set; }

    [Column("qr")]
    public string? QrCode { get; set; }

    [Column("qrpath")]
    public string? QrPath { get; set; }

    [Column("size")]
    public string? ImageSize { get; set; }

    [Column("location")]
    public string? Location { get; set; }

    [Column("hasqr")]
    public bool IsGenerated { get; set; }

    [Column("createddate")]
    public string? GeneratedDate { get; set; }

    // UI-only properties — not stored in Supabase
    public ImageSource? ImagePreview { get; set; }
    public byte[]? ImageBytes { get; set; }

    public void NormalizeName() {
        Name = Normalize.NormalizeStrings(Name);
    }
}