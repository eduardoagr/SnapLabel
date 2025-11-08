using System.Text.Json;

namespace SnapLabel.ViewModels;

public partial class AuthenticationPageViewModel : ObservableObject {

    private readonly Client _client;
    private readonly IShellService _shellService;
    private readonly ISecureStorage _secureStorage;

    [ObservableProperty]
    public partial UserViewModel UserVM { get; set; }

    [ObservableProperty]
    public partial bool IsCreatingAccountPopUpOpen { get; set; }

    public AuthenticationPageViewModel(Client client, IShellService shellService, ISecureStorage secureStorage) {
        _client = client;
        _shellService = shellService;
        _secureStorage = secureStorage;
        UserVM = new UserViewModel(new User());
        UserVM.UserPropertiesChanged += UserVM_UserPropertiesChanged;
    }

    private void UserVM_UserPropertiesChanged() {

        CreateAccountCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand]
    void OpenCreateAccountPopUp() {

        UserVM.UserPropertiesChanged -= UserVM_UserPropertiesChanged;
        UserVM = new UserViewModel(new User());
        UserVM.UserPropertiesChanged += UserVM_UserPropertiesChanged;

        IsCreatingAccountPopUpOpen = true;
        CreateAccountCommand.NotifyCanExecuteChanged();

    }

    [RelayCommand]
    async Task Login() {

        await _shellService.NavigateToAsync($"//{nameof(DashboardPage)}");
    }

    [RelayCommand]
    void Cancel() {

        UserVM.UserPropertiesChanged -= UserVM_UserPropertiesChanged;
        IsCreatingAccountPopUpOpen = false;
        CreateAccountCommand.NotifyCanExecuteChanged();
    }

    [RelayCommand(CanExecute = nameof(CanCreateAccount))]
    async Task CreateAccount() {
        try {
            var session = await _client.Auth.SignUp(UserVM.Email, UserVM.Password);

            if(session?.User?.Id is not null) {
                IsCreatingAccountPopUpOpen = false;
                await _shellService.NavigateToAsync($"//{AppConstants.HOME}");

                await _secureStorage.SetAsync(AppConstants.EMAIL, UserVM.Email);
                await _secureStorage.SetAsync(AppConstants.PASSWORD, UserVM.Password);
            }
        } catch(Exception ex) {
            SupabaseErrorResponse? errorResponse;
            try {
                errorResponse = JsonSerializer.Deserialize<SupabaseErrorResponse>(ex.Message);
            } catch {
                await _shellService.DisplayAlertAsync("Error", "An unexpected error occurred.", "OK");
                return;
            }

            var message = SupabaseErrorMessage.GetErrorMessage(errorResponse?.ErrorCode ?? "");
            await _shellService.DisplayAlertAsync("Error", message, "OK");
        }
    }

    public bool CanCreateAccount => UserVM.CanSave;

}