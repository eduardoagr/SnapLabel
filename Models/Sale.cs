namespace SnapLabel.Models;

[Table("sales")]
public class Sale : BaseModel, IHasId {

    [PrimaryKey("id")]

    public Guid id { get; set; }

    public Guid product_id { get; set; }

    public Guid store_id { get; set; }

    public int quantity { get; set; }

    public decimal unit_price { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }
}
