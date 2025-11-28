
namespace SnapLabel.Services;

public class ShellService : IShellService {

    public Task DisplayAlertAsync(string title, string message, string cancel) => Shell.Current.DisplayAlertAsync(title, message, cancel);
    

    public Task<bool> DisplayConfirmAsync(string title, string message, string accept, string cancel) =>
        Shell.Current.DisplayAlertAsync(title, message, accept, cancel);
    

    public async Task DisplayToastAsync(string message, ToastDuration toastDuration, double fontSize = 14) {
        var cancellationTokenSource = new CancellationTokenSource();
        var toast = Toast.Make(message, toastDuration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
    }

    public Task NavigateBackAsync() => Shell.Current.GoToAsync("..", true);
    

    public Task NavigateToAsync(string route) => Shell.Current.GoToAsync(route, true);


    public Task NavigateToAsync(string route, IDictionary<string, object> parameters) => Shell.Current.GoToAsync(route, parameters);
   
}