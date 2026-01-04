namespace SnapLabel.Controls;

public static class ToolbarItemExtensions {

    public static readonly BindableProperty IsVisibleProperty =
        BindableProperty.CreateAttached(
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
            var page = await WaitForParentAsync(item);
            if(page == null)
                return;

            bool visible = (bool)newValue;
            if(visible) {
                if(!page.ToolbarItems.Contains(item))
                    page.ToolbarItems.Add(item);
            } else {
                page.ToolbarItems.Remove(item);
            }
        });
    }

    private static async Task<ContentPage?> WaitForParentAsync(ToolbarItem item, int retries = 5) {
        for(int i = 0 ; i < retries ; i++) {
            if(item.Parent is ContentPage page)
                return page;

            await Task.Delay(10);
        }
        return item.Parent as ContentPage;
    }
}