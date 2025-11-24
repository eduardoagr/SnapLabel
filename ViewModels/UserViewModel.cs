namespace SnapLabel.ViewModels;

public partial class UserViewModel(User user, IMessenger messenger) : ObservableObject {

    [ObservableProperty]
    public partial string Username { get; set; } = user.Username ?? string.Empty;

    [ObservableProperty]
    public partial string Email { get; set; } = user.Email ?? string.Empty;

    [ObservableProperty]
    public partial string Password { get; set; } = string.Empty;

    partial void OnUsernameChanged(string value) => messenger.Send(new FieldsChangedMessage());

    partial void OnEmailChanged(string value) => messenger.Send(new FieldsChangedMessage());

    partial void OnPasswordChanged(string value) => messenger.Send(new FieldsChangedMessage());

}