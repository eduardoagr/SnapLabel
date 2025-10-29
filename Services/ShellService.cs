namespace SnapLabel.Services;

public class ShellService : IShellService {

    public Task DisplayAlertAsync(string title, string message, string cancel) {
        return Shell.Current.DisplayAlert(title, message, cancel);
    }

    public async Task DisplayToastAsync(string message, ToastDuration toastDuration, double fontSize = 14) {
        var cancellationTokenSource = new CancellationTokenSource();
        var toast = Toast.Make(message, toastDuration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
    }

    public Task NavigateBackAsync() {
        return Shell.Current.GoToAsync("..", true);
    }

    public Task NavigateToAsync(string route) {
        return Shell.Current.GoToAsync(route, true);
    }
}