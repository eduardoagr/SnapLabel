namespace SnapLabel.Models; 

public partial class Employee : ObservableObject {

    public string? Id { get; set; }

    [ObservableProperty] 
    public partial string? Name { get; set; }
    
    [ObservableProperty] 
    public partial string? Email { get; set; }
   
    [ObservableProperty] 
    public partial string? Role { get; set; }
   
    [ObservableProperty] 
    public partial string? Phone { get; set; }
}
