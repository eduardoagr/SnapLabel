namespace SnapLabel.Interfaces;

public interface IPrintingPopupService {

    Task ShowAsync(string message, string imageSource);

    Task HideAsync();
}
