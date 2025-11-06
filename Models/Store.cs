namespace SnapLabel.Models;

public class Store : BaseModel {

    [PrimaryKey("id", false)]
    public long Id { get; set; }

    [Column("store_id")]
    public long Store_id { get; set; }

}
