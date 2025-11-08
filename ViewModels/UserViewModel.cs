namespace SnapLabel.ViewModels;

public partial class UserViewModel : ObservableObject {

    public event Action? UserPropertiesChanged;

    private readonly User _user;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveToModelCommand))]
    public partial string Name { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveToModelCommand))]
    public partial string Email { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(SaveToModelCommand))]
    public partial string Password { get; set; } = string.Empty; // Only used for transmission

    public UserViewModel(User user) {
        _user = user;

        Name = user.Name ?? string.Empty;
        Email = user.Email ?? string.Empty;

        PropertyChanged += (s, e) => UserPropertiesChanged?.Invoke();
    }

    public bool CanSave =>
        !string.IsNullOrWhiteSpace(Name)
        && !string.IsNullOrWhiteSpace(Email)
        && !string.IsNullOrWhiteSpace(Password); // Required for transmission, not storage

    [RelayCommand(CanExecute = nameof(CanSave))]
    public void SaveToModel() {
        _user.Name = Name;
        _user.Email = Email;
    }

    public User GetUser() {
        SaveToModel();
        return _user;
    }

}
