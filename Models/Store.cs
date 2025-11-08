namespace SnapLabel.Models;


[Table("stores")]
public class Store : BaseModel {

    [PrimaryKey("id")]
    public long Id { get; set; }

    [Column("store_name")]
    public string? StoreName { get; set; }

    [Column("owner_email")]
    public string? OwnerEmail { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("location")]
    public string? Location { get; set; }

    [Column("created_date")]
    public DateTime CreatedDate { get; set; }

    [Column("is_active")]
    public bool IsActive { get; set; }
}
