namespace SnapLabel.Models;

[Table("products")]
public class Product : BaseModel {

    [PrimaryKey("id")]
    public Guid id { get; set; }

    public string? name { get; set; }

    public decimal price { get; set; }

    public string? image_url { get; set; }

    public Guid store_id { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }
}
