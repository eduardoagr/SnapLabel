namespace SnapLabel.Interfaces {

    public interface IShellService {

        Task DisplayAlertAsync(string title, string message, string cancel);

        Task NavigateBackAsync();

        Task NavigateToAsync(string route);
    }
}
