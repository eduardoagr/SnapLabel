namespace SnapLabel.Models;


public class User : IFirebaseEntity {

    public string? Id { get; set; }

    public string? Username { get; set; }

    public string? Email { get; set; }

    public string? StoreId { get; set; }

    public string? CreatedAt { get; set; } = DateTime.Now.ToString("f");

    public string? UpdatedAt { get; set; } = DateTime.Now.ToString("f");
}

