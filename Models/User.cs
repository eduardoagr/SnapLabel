namespace SnapLabel.Models;

[Table("users")]
public class User : BaseModel {

    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("name")]
    public string? Name { get; set; }

    [Column("email")]
    public string? Email { get; set; }

    [Column("role")]
    public string Role { get; set; } = "viewer"; // viewer, manager, admin

    [Column("created_date")]
    public DateTime CreatedDate { get; set; }

    [Column("store_id")]
    public long StoreId { get; set; }

}
