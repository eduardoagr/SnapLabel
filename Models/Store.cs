namespace SnapLabel.Models;


public class Store : IFirebaseEntity {

    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? UserId { get; set; }

    public decimal TotalRevenue { get; set; }

    public decimal TotalSales { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}

