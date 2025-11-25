namespace SnapLabel.ViewModels;

public partial class AuthenticationPageViewModel : ObservableObject {

    private readonly IFirebaseAuthClient firebaseAuthClient;
    private readonly IShellService _shellService;
    private readonly IMessenger _messenger;
    private readonly ICustomDialogService _customDialogService;
    private readonly IDatabaseService<User> _databaseService;

    [ObservableProperty]
    public partial UserViewModel UserVM { get; set; }

    [ObservableProperty]
    public partial bool IsCreatingAccountPopUpOpen { get; set; }


    public AuthenticationPageViewModel(IFirebaseAuthClient authClient, IShellService shellService,
        IMessenger messenger, ICustomDialogService customDialogService, IDatabaseService<User> databaseService) {

        firebaseAuthClient = authClient;
        _shellService = shellService;
        _messenger = messenger;
        _customDialogService = customDialogService;

        _messenger.Register<FieldsChangedMessage>(this, (_, _) => {
            LoginCommand.NotifyCanExecuteChanged();
            CreateAccountCommand.NotifyCanExecuteChanged();
        });

        UserVM = new UserViewModel(new User(), _messenger);
        _databaseService = databaseService;
    }

    [RelayCommand]
    async Task CheckAuth() {

        if(firebaseAuthClient.User is not null) {

            await Task.Delay(100);
            await _shellService.NavigateToAsync($"//{AppConstants.HOME}");
        }
    }

    [RelayCommand]
    void OpenCreateAccountPopUp() {
        UserVM = new UserViewModel(new User(), _messenger); // reset fields
        IsCreatingAccountPopUpOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    async Task Login() {

        await _customDialogService.ShowAsync("Please wait", "loading.gif");

        try {

            await firebaseAuthClient.SignInWithEmailAndPasswordAsync(UserVM.Email, UserVM.Password);

            await _shellService.NavigateToAsync($"//{AppConstants.HOME}");

            await _customDialogService.HideAsync();

        } catch(FirebaseAuthException ex) {

            await DisplayErrorAsync(ex);

            await _customDialogService.HideAsync();
        }

    }

    [RelayCommand]
    void Cancel() {
        IsCreatingAccountPopUpOpen = false;
    }

    [RelayCommand(CanExecute = nameof(CanCreateAccount))]
    async Task CreateAccount() {
        await _customDialogService.ShowAsync("Creating your account", "loading.gif");

        try {

            await firebaseAuthClient.CreateUserWithEmailAndPasswordAsync(UserVM.Email, UserVM.Password, UserVM.Username);

            var user = new User {

                Email = UserVM.Email,
                CreatedAt = DateTime.Now.ToString("f"),
                UpdatedAt = DateTime.Now.ToString("f"),
                StoreId = string.Empty,
                Id = string.Empty,
            };

            var id = await _databaseService.InsertAsync(user);

            await _databaseService.UpdateAsync(user);

            await _customDialogService.HideAsync();

            await _shellService.NavigateToAsync($"//{AppConstants.HOME}");

        } catch(FirebaseAuthException ex) {

            await DisplayErrorAsync(ex);

            await _customDialogService.HideAsync();
        }

    }

    private bool CanLogin() =>
        !string.IsNullOrWhiteSpace(UserVM.Email) &&
        !string.IsNullOrWhiteSpace(UserVM.Password);

    private bool CanCreateAccount =>
        !string.IsNullOrWhiteSpace(UserVM.Email) &&
        !string.IsNullOrWhiteSpace(UserVM.Password) &&
        !string.IsNullOrWhiteSpace(UserVM.Username);

    private async Task DisplayErrorAsync(FirebaseAuthException ex) {

        switch(ex.Reason) {

            case AuthErrorReason.Undefined:
            await _shellService.DisplayAlertAsync("Error", "An undefined error occurred.", "OK");
            break;
            case AuthErrorReason.Unknown:
            await _shellService.DisplayAlertAsync("Error", "An unknown error occurred. Please try again.", "OK");
            break;
            case AuthErrorReason.OperationNotAllowed:
            await _shellService.DisplayAlertAsync("Error", "This operation is not allowed. Check your Firebase settings.", "OK");
            break;
            case AuthErrorReason.UserDisabled:
            await _shellService.DisplayAlertAsync("Account Disabled", "This user account has been disabled.", "OK");
            break;
            case AuthErrorReason.UserNotFound:
            await _shellService.DisplayAlertAsync("User Not Found", "No account found with this email.", "OK");
            break;
            case AuthErrorReason.InvalidProviderID:
            await _shellService.DisplayAlertAsync("Error", "Invalid provider ID specified.", "OK");
            break;
            case AuthErrorReason.InvalidAccessToken:
            await _shellService.DisplayAlertAsync("Access Denied", "Invalid access token. Please sign in again.", "OK");
            break;
            case AuthErrorReason.LoginCredentialsTooOld:
            await _shellService.DisplayAlertAsync("Session Expired", "Your login credentials are outdated. Please reauthenticate.", "OK");
            break;
            case AuthErrorReason.MissingRequestURI:
            await _shellService.DisplayAlertAsync("Error", "Missing request URI. Please check your configuration.", "OK");
            break;
            case AuthErrorReason.SystemError:
            await _shellService.DisplayAlertAsync("System Error", "A system error occurred. Try again later.", "OK");
            break;
            case AuthErrorReason.InvalidEmailAddress:
            await _shellService.DisplayAlertAsync("Invalid Email", "The email address format is invalid.", "OK");
            break;
            case AuthErrorReason.MissingPassword:
            await _shellService.DisplayAlertAsync("Missing Password", "Please enter a password.", "OK");
            break;
            case AuthErrorReason.WeakPassword:
            await _shellService.DisplayAlertAsync("Weak Password", "Password is too weak. Use at least 6 characters.", "OK");
            break;
            case AuthErrorReason.EmailExists:
            await _shellService.DisplayAlertAsync("Email Exists", "An account with this email already exists.", "OK");
            break;
            case AuthErrorReason.MissingEmail:
            await _shellService.DisplayAlertAsync("Missing Email", "Please enter an email address.", "OK");
            break;
            case AuthErrorReason.UnknownEmailAddress:
            await _shellService.DisplayAlertAsync("Unknown Email", "This email address is not recognized.", "OK");
            break;
            case AuthErrorReason.WrongPassword:
            await _shellService.DisplayAlertAsync("Wrong Password", "The password entered is incorrect.", "OK");
            break;
            case AuthErrorReason.TooManyAttemptsTryLater:
            await _shellService.DisplayAlertAsync("Too Many Attempts", "Too many failed attempts. Please try again later.", "OK");
            break;
            case AuthErrorReason.MissingRequestType:
            await _shellService.DisplayAlertAsync("Error", "Missing request type. Please check your request.", "OK");
            break;
            case AuthErrorReason.ResetPasswordExceedLimit:
            await _shellService.DisplayAlertAsync("Limit Exceeded", "Password reset limit exceeded. Try again later.", "OK");
            break;
            case AuthErrorReason.InvalidIDToken:
            await _shellService.DisplayAlertAsync("Invalid Token", "Your ID token is invalid or expired.", "OK");
            break;
            case AuthErrorReason.MissingIdentifier:
            await _shellService.DisplayAlertAsync("Missing Identifier", "Missing user identifier. Please check your request.", "OK");
            break;
            case AuthErrorReason.InvalidIdentifier:
            await _shellService.DisplayAlertAsync("Invalid Identifier", "The user identifier is invalid.", "OK");
            break;
            case AuthErrorReason.AlreadyLinked:
            await _shellService.DisplayAlertAsync("Already Linked", "This account is already linked to another credential.", "OK");
            break;
            case AuthErrorReason.InvalidApiKey:
            await _shellService.DisplayAlertAsync("Invalid API Key", "The API key is invalid. Check your Firebase config.", "OK");
            break;
            case AuthErrorReason.AccountExistsWithDifferentCredential:
            await _shellService.DisplayAlertAsync("Credential Conflict", "An account already exists with a different credential.", "OK");
            break;
            default:
            await _shellService.DisplayAlertAsync("Error", $"Unexpected error: {ex.Reason}", "OK");
            break;

        }
    }
}