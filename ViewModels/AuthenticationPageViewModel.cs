namespace SnapLabel.ViewModels;

public partial class AuthenticationPageViewModel : BasePageViewModel<User> {

    private readonly IFirebaseAuthClient _firebaseAuthClient;
    private readonly ICustomDialogService _customDialogService;

    public AuthenticationPageViewModel(
        IMessenger messenger,
        IFirebaseAuthClient authClient,
        IShellService shellService,
        ICustomDialogService customDialogService,
        IDatabaseService<User> databaseService)
        : base(shellService, databaseService, customDialogService, messenger) {

        _firebaseAuthClient = authClient;
        _customDialogService = customDialogService;

        User = new User();

        TrackModel(User, LoginCommand, CreateAccountCommand);
    }

    [ObservableProperty]
    public partial User User { get; set; }

    [ObservableProperty]
    public partial bool _isCreatingAccountPopUpOpen { get; set; }

    [RelayCommand]
    async Task CheckAuth() {
        if(_firebaseAuthClient.User is not null) {
            await Task.Delay(100); // small delay for smoother navigation
            await NavigateAsync($"//{AppConstants.HOME}");
        }
    }
    [RelayCommand]
    void Cancel() {
        _isCreatingAccountPopUpOpen = false;
    }

    [RelayCommand]
    void OpenCreateAccountPopUp() {
        User = new User(); // reset fields
        _isCreatingAccountPopUpOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    async Task Login() {
        await _customDialogService.ShowAsync("Please wait", "loading.gif");
        try {
            await _firebaseAuthClient.SignInWithEmailAndPasswordAsync(User.Email, User.Password);
            await NavigateAsync($"//{AppConstants.HOME}");
        } catch(FirebaseAuthException ex) {
            await DisplayErrorAsync(ex);
        } finally {
            await _customDialogService.HideAsync();
        }
    }

    [RelayCommand(CanExecute = nameof(CanCreateAccount))]
    async Task CreateAccount() {
        await _customDialogService.ShowAsync("Creating your account", "loading.gif");
        try {
            await _firebaseAuthClient.CreateUserWithEmailAndPasswordAsync(User.Email, User.Password, User.Username);

            var user = new User {
                Email = User.Email,
                Username = User.Username,
                PhoneNumber = User.PhoneNumber,
                StoreId = string.Empty,
                Id = string.Empty,
            };

            await DatabaseService.InsertAsync(user);
            await NavigateAsync($"//{AppConstants.HOME}");
        } catch(FirebaseAuthException ex) {
            await DisplayErrorAsync(ex);
        } finally {
            await _customDialogService.HideAsync();
        }
    }

    private bool CanLogin() =>
        Validation.AllFilled(User.Email, User.Password);

    private bool CanCreateAccount() =>
        Validation.AllFilled(User.Email, User.Password, User.Username, User.PhoneNumber);

    partial void OnUserChanged(User value) {
        if(value is null)
            return;
        TrackModel(value, LoginCommand, CreateAccountCommand);

    }
    private async Task DisplayErrorAsync(FirebaseAuthException ex) {

        switch(ex.Reason) {

            case AuthErrorReason.Undefined:
            await DisplayAlertAsync("Error", "An undefined error occurred.", "OK");
            break;
            case AuthErrorReason.Unknown:
            await DisplayAlertAsync("Error", "An unknown error occurred. Please try again.", "OK");
            break;
            case AuthErrorReason.OperationNotAllowed:
            await DisplayAlertAsync("Error", "This operation is not allowed. Check your Firebase settings.", "OK");
            break;
            case AuthErrorReason.UserDisabled:
            await DisplayAlertAsync("Account Disabled", "This user account has been disabled.", "OK");
            break;
            case AuthErrorReason.UserNotFound:
            await DisplayAlertAsync("User Not Found", "No account found with this email.", "OK");
            break;
            case AuthErrorReason.InvalidProviderID:
            await DisplayAlertAsync("Error", "Invalid provider ID specified.", "OK");
            break;
            case AuthErrorReason.InvalidAccessToken:
            await DisplayAlertAsync("Access Denied", "Invalid access token. Please sign in again.", "OK");
            break;
            case AuthErrorReason.LoginCredentialsTooOld:
            await DisplayAlertAsync("Session Expired", "Your login credentials are outdated. Please reauthenticate.", "OK");
            break;
            case AuthErrorReason.MissingRequestURI:
            await DisplayAlertAsync("Error", "Missing request URI. Please check your configuration.", "OK");
            break;
            case AuthErrorReason.SystemError:
            await DisplayAlertAsync("System Error", "A system error occurred. Try again later.", "OK");
            break;
            case AuthErrorReason.InvalidEmailAddress:
            await DisplayAlertAsync("Invalid Email", "The email address format is invalid.", "OK");
            break;
            case AuthErrorReason.MissingPassword:
            await DisplayAlertAsync("Missing Password", "Please enter a password.", "OK");
            break;
            case AuthErrorReason.WeakPassword:
            await DisplayAlertAsync("Weak Password", "Password is too weak. Use at least 6 characters.", "OK");
            break;
            case AuthErrorReason.EmailExists:
            await DisplayAlertAsync("Email Exists", "An account with this email already exists.", "OK");
            break;
            case AuthErrorReason.MissingEmail:
            await DisplayAlertAsync("Missing Email", "Please enter an email address.", "OK");
            break;
            case AuthErrorReason.UnknownEmailAddress:
            await DisplayAlertAsync("Unknown Email", "This email address is not recognized.", "OK");
            break;
            case AuthErrorReason.WrongPassword:
            await DisplayAlertAsync("Wrong Password", "The password entered is incorrect.", "OK");
            break;
            case AuthErrorReason.TooManyAttemptsTryLater:
            await DisplayAlertAsync("Too Many Attempts", "Too many failed attempts. Please try again later.", "OK");
            break;
            case AuthErrorReason.MissingRequestType:
            await DisplayAlertAsync("Error", "Missing request type. Please check your request.", "OK");
            break;
            case AuthErrorReason.ResetPasswordExceedLimit:
            await DisplayAlertAsync("Limit Exceeded", "Password reset limit exceeded. Try again later.", "OK");
            break;
            case AuthErrorReason.InvalidIDToken:
            await DisplayAlertAsync("Invalid Token", "Your ID token is invalid or expired.", "OK");
            break;
            case AuthErrorReason.MissingIdentifier:
            await DisplayAlertAsync("Missing Identifier", "Missing user identifier. Please check your request.", "OK");
            break;
            case AuthErrorReason.InvalidIdentifier:
            await DisplayAlertAsync("Invalid Identifier", "The user identifier is invalid.", "OK");
            break;
            case AuthErrorReason.AlreadyLinked:
            await DisplayAlertAsync("Already Linked", "This account is already linked to another credential.", "OK");
            break;
            case AuthErrorReason.InvalidApiKey:
            await DisplayAlertAsync("Invalid API Key", "The API key is invalid. Check your Firebase config.", "OK");
            break;
            case AuthErrorReason.AccountExistsWithDifferentCredential:
            await DisplayAlertAsync("Credential Conflict", "An account already exists with a different credential.", "OK");
            break;
            default:
            await DisplayAlertAsync("Error", $"Unexpected error: {ex.Reason}", "OK");
            break;

        }
    }
}