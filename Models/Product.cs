

namespace SnapLabel.Models;

[Table("products")]
public class Product : BaseModel {

    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("price")]
    public string? Price { get; set; }

    [Column("location")]
    public string? Location { get; set; }

    [Column("imagebytes")]
    public byte[]? ImageBytes { get; set; }

    [Column("createddate")]
    public string GeneratedDate { get; set; } = DateTime.Now.ToString("f");

    [Column("store_id")]
    public long Store_id { get; set; }


    public void NormalizeValues() {
        Name = Normalize.NormalizeStrings(Name);
        Location = Normalize.NormalizeStrings(Location);
    }
}