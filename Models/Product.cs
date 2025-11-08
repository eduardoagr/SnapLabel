

namespace SnapLabel.Models;

[Table("products")]
public class Product : BaseModel {

    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("price")]
    public decimal? Price { get; set; }

    [Column("location")]
    public string? Location { get; set; }

    [Column("image_url")]
    public string? ImageUrl { get; set; }

    [Column("store_id")]
    public long StoreId { get; set; }



    public void NormalizeValues() {
        Name = Normalize.NormalizeStrings(Name);
        Location = Normalize.NormalizeStrings(Location);
    }
}