namespace SnapLabel.ViewModels;

public partial class AuthenticationPageViewModel : ObservableObject {

    private readonly Client _client;
    private readonly IShellService _shellService;
    private readonly ISecureStorage _secureStorage;
    private readonly IMessenger _messenger;
    private readonly ICustomDialogService _customDialogService;
    private readonly IDatabaseService<User> _databaseService;

    [ObservableProperty]
    public partial UserViewModel UserVM { get; set; }

    [ObservableProperty]
    public partial bool IsCreatingAccountPopUpOpen { get; set; }


    public AuthenticationPageViewModel(Client client, IShellService shellService,
        ISecureStorage secureStorage, IMessenger messenger, ICustomDialogService customDialogService, IDatabaseService<User> databaseService) {

        _client = client;
        _shellService = shellService;
        _secureStorage = secureStorage;
        _messenger = messenger;
        _customDialogService = customDialogService;

        _messenger.Register<FieldsChangedMessage>(this, (_, _) => {
            LoginCommand.NotifyCanExecuteChanged();
            CreateAccountCommand.NotifyCanExecuteChanged();
        });

        UserVM = new UserViewModel(new User(), _messenger);
        _databaseService = databaseService;
    }

    public async Task CheckAuth() {

        string? email = await _secureStorage.GetAsync(AppConstants.EMAIL);
        string? password = await CredentialVault.RetrievePasswordAsync();

        if(!string.IsNullOrEmpty(email) && !string.IsNullOrEmpty(password)) {
            try {

                Supabase.Gotrue.Session? auth = await _client.Auth.SignIn(email, password);

                await _shellService.NavigateToAsync($"//{AppConstants.HOME}");

            } catch(Exception ex) {

                await SupabaseErrorHelper.HandleAsync(ex, _shellService);
            }
        }
    }

    [RelayCommand]
    void OpenCreateAccountPopUp() {
        UserVM = new UserViewModel(new User(), _messenger); // reset fields
        IsCreatingAccountPopUpOpen = true;
    }

    [RelayCommand(CanExecute = nameof(CanLogin))]
    async Task Login() {
        try {

            await _customDialogService.ShowAsync("Please wait", "loading.gif");

            Supabase.Gotrue.Session? auth = await _client.Auth.SignIn(UserVM.Email, UserVM.Password);

            if(auth?.User is not null) {
                await _secureStorage.SetAsync(AppConstants.EMAIL, UserVM.Email);
                await CredentialVault.StorePasswordAsync(UserVM.Password);

                await _customDialogService.HideAsync();

                User user = new() {
                    id = Guid.Parse(auth.User.Id!), // ✅ Explicit conversion
                    email = auth.User.Email,
                    name = UserVM.Name
                };

                await _databaseService.InsertAsync(user);

                await _shellService.NavigateToAsync($"//{AppConstants.HOME}");
            }
        } catch(Exception ex) {

            await _customDialogService.HideAsync();

            await SupabaseErrorHelper.HandleAsync(ex, _shellService);
        }
    }

    [RelayCommand]
    void Cancel() {
        IsCreatingAccountPopUpOpen = false;
    }

    [RelayCommand(CanExecute = nameof(CanCreateAccount))]
    async Task CreateAccount() {
        try {

            await _customDialogService.ShowAsync("Please wait", "loading.gif");

            Supabase.Gotrue.Session? session = await _client.Auth.SignUp(UserVM.Email, UserVM.Password);

            if(session?.User?.Id is not null) {
                IsCreatingAccountPopUpOpen = false;
                await _shellService.NavigateToAsync($"//{AppConstants.HOME}");

                await _customDialogService.HideAsync();

                if(!Guid.TryParse(session.User.Id, out Guid parsedId))
                    throw new Exception("Invalid Auth UID format");

                User user = new() {

                    id = parsedId,
                    email = session.User.Email,
                    name = UserVM.Name,
                    created_at = DateTime.UtcNow
                };

                await _databaseService.InsertAsync(user);

                await _secureStorage.SetAsync(AppConstants.EMAIL, session?.User?.Email ?? string.Empty);
                await CredentialVault.StorePasswordAsync(UserVM.Password);
            }
        } catch(Exception ex) {
            await _customDialogService.HideAsync();

            await SupabaseErrorHelper.HandleAsync(ex, _shellService);
        }
    }

    private bool CanLogin() =>
        !string.IsNullOrWhiteSpace(UserVM.Email) &&
        !string.IsNullOrWhiteSpace(UserVM.Password);

    private bool CanCreateAccount =>
        !string.IsNullOrWhiteSpace(UserVM.Email) &&
        !string.IsNullOrWhiteSpace(UserVM.Password) &&
        !string.IsNullOrWhiteSpace(UserVM.Name);
}
