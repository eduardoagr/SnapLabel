namespace SnapLabel.Models;

[Table("users")]
public class User : BaseModel, IHasId {


    [PrimaryKey("id")]
    public Guid id { get; set; }

    public string? name { get; set; }

    public string? email { get; set; }

    public string? store_id { get; set; }

    public DateTime created_at { get; set; }

    public DateTime? updated_at { get; set; }
}
