namespace SnapLabel.Models;

[Table("store_users")]
public class StoreUser : BaseModel, IHasId {

    [PrimaryKey("id")]
    public Guid id { get; set; }

    public Guid store_id { get; set; }

    public Guid product_id { get; set; }

    public Guid user_id { get; set; }

    public DateTime created_at { get; set; }

    public DateTime updated_at { get; set; }
}
