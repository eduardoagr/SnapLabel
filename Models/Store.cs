namespace SnapLabel.Models;

public partial class Store : ObservableObject, IFirebaseEntity {

    public string? Id { get; set; }

    [ObservableProperty]
    public partial string? Name { get; set; }

    public string? ManagerId { get; set; }

    public string? ManagerUsername { get; set; }

    public string? ManagerEmail { get; set; }

    public string? TotalRevenue { get; set; }

    public string? TotalSales { get; set; }

    [ObservableProperty]
    public partial string? Address { get; set; }

    [ObservableProperty]
    public partial string? Phones { get; set; }

    [ObservableProperty]
    public partial string? StoreEmail { get; set; }

    [ObservableProperty]
    public partial List<Employee> Employees { get; set; } = [];
}

