

using Syncfusion.Maui.Popup;

namespace SnapLabel.Services;

public class PrintingPopupService : IPrintingPopupService {

    SfPopup popup;

    public PrintingPopupService() {

        popup = new SfPopup {
            AnimationMode = PopupAnimationMode.None,
            AutoSizeMode = PopupAutoSizeMode.Both,
            StaysOpen = true,
            ShowHeader = false,
            PopupStyle = new PopupStyle {

                CornerRadius = 16
                // Background will be set dynamically in ShowAsync
            }
        };
    }

    public Task HideAsync() {

        Debug.WriteLine($"[Popup] Dismissing from: {popup.Parent?.GetType().Name}");
        popup.Dismiss();
        return Task.CompletedTask;
    }

    public Task ShowAsync(string message, string imageSource) {

        Debug.WriteLine($"[Popup] Showing on: {popup.Parent?.GetType().Name}");
        Debug.WriteLine($"[Popup] IsOpen: {popup.IsOpen}");

        // Update background based on current theme
        popup.PopupStyle.PopupBackground = Application.Current!.RequestedTheme == AppTheme.Dark
            ? Color.FromArgb("#232323")
            : Colors.White;

        // Update content with custom message and image

        popup.ContentTemplate = new DataTemplate(() => {
            return new VerticalStackLayout {
                Padding = 10,
                Children =
                {
                    new Image { Source = "wait.gif", IsAnimationPlaying = true },
                    new Label
                    {
                        Text = message,
                        FontAttributes = FontAttributes.Bold,
                        FontSize = 18,
                        HorizontalTextAlignment = TextAlignment.Center
                    }
                }
            };
        });

        popup.Show();
        return Task.CompletedTask;
    }
}