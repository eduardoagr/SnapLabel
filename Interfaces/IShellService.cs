namespace SnapLabel.Interfaces {

    public interface IShellService {

        Task DisplayAlertAsync(string title, string message, string cancel);

        Task DisplayToast(string message, ToastDuration toastDuration, double fontSize = 14);

        Task NavigateBackAsync();

        Task NavigateToAsync(string route);
    }
}
