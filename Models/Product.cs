namespace SnapLabel.Models;


public partial class Product : IFirebaseEntity {

    public string? Id { get; set; }

    public string? Name { get; set; }

    public decimal Price { get; set; }

    public string? ImageUrl { get; set; }

    public string? QrUrl { get; set; }

    public string? StoreId { get; set; }

    public string? CreatedAt { get; set; }

    public string? UpdatedAt { get; set; }
}
