
namespace SnapLabel.ViewModels {

    public class NewStorePageViewModel : ObservableObject {

        private readonly IShellService _shellService;
        private readonly IDatabaseService<User> _databaseService;

        public List<User> Users { get; set; } = [];

        public NewStorePageViewModel(IShellService shellService, IDatabaseService<User> databaseService) {

            _shellService = shellService;
            _databaseService = databaseService;
            GetUsers("Users");
        }

        private async void GetUsers(string Node) {
            var items = await _databaseService.GetAllAsync<User>("Users");

            Users = items.ToList();
        }
    }
}
