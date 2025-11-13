namespace SnapLabel.Models;

[Table("stores")]
public class Store : BaseModel, IHasId {

    [PrimaryKey("id")]
    public Guid id { get; set; }

    public string? name { get; set; }

    public Guid user_id { get; set; }

    public decimal total_revenue { get; set; }

    public decimal total_sales { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }
}
