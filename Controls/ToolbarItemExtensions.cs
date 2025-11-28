namespace SnapLabel.Controls;

public static class ToolbarItemExtensions {

    public static readonly BindableProperty IsVisibleProperty =
        BindableProperty.Create(
            "IsVisible",
            typeof(bool),
            typeof(ToolbarItemExtensions),
            true,
            propertyChanged: OnIsVisibleChanged);

    public static bool GetIsVisible(BindableObject view)
        => (bool)view.GetValue(IsVisibleProperty);

    public static void SetIsVisible(BindableObject view, bool value)
        => view.SetValue(IsVisibleProperty, value);

    private static void OnIsVisibleChanged(BindableObject bindable, object oldValue, object newValue) {
        if(bindable is not ToolbarItem item)
            return;

        item.Dispatcher?.Dispatch(async () => {
            if(item.Parent is not ContentPage page) {
                await Task.Delay(10); // tiny delay
                if(item.Parent is not ContentPage retryPage)
                    return;
                page = retryPage;
            }

            bool visible = (bool)newValue;
            if(visible) {
                if(!page.ToolbarItems.Contains(item))
                    page.ToolbarItems.Add(item);
            }
            else {
                page.ToolbarItems.Remove(item);
            }
        });
    }
}