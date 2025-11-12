namespace SnapLabel.Interfaces;

public interface ICustomDialogService {

    Task ShowAsync(string message, string imageSource);

    Task HideAsync();
}
