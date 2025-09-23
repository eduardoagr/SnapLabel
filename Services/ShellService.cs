namespace SnapLabel.Services {

    public class ShellService : IShellService {

        public Task DisplayAlertAsync(string title, string message, string cancel) {
            return Shell.Current.DisplayAlert(title, message, cancel);
        }

        public Task NavigateBackAsync() {
            return Shell.Current.GoToAsync("..", true);
        }

        public Task NavigateToAsync(string route) {
            return Shell.Current.GoToAsync(route, true);
        }
    }
}