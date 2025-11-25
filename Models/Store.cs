namespace SnapLabel.Models;

public class Store : IFirebaseEntity {

    public string? Id { get; set; }

    public string? Name { get; set; }

    public string? ManagerId { get; set; }

    public decimal TotalRevenue { get; set; }

    public decimal TotalSales { get; set; }

    public string? Address { get; set; }

    public string? Phones { get; set; }

    public string? CreatedAt { get; set; }

    public string? UpdatedAt { get; set; }
}

