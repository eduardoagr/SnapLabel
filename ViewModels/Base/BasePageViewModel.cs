namespace SnapLabel.ViewModels.Base;

public abstract partial class BasePageViewModel<T>(
    IShellService shellService,
    IDatabaseService<T> databaseService,
    ICustomDialogService customDialogService,
    IMessenger messenger) : ObservableObject
    where T : class, IFirebaseEntity, INotifyPropertyChanged {


    protected readonly IShellService ShellService = shellService;
    protected readonly IDatabaseService<T> DatabaseService = databaseService;
    protected readonly IMessenger Messenger = messenger;
    protected readonly ICustomDialogService CustomDialogService = customDialogService;

    private PropertyChangedEventHandler? _propertyChangedHandler;
    private INotifyPropertyChanged? _trackedModel;

    // 🔹 Call this once in your constructor
    protected void TrackModel(T model, params IRelayCommand[] commands) {

        // If we were tracking a previous model, unsubscribe cleanly
        if(_trackedModel != null && _propertyChangedHandler != null)
            _trackedModel.PropertyChanged -= _propertyChangedHandler;

        // Create a single handler so unsubscribing works
        _propertyChangedHandler = (_, _) => {
            foreach(var cmd in commands)
                cmd.NotifyCanExecuteChanged();
        };

        // Subscribe to the NEW model
        model.PropertyChanged += _propertyChangedHandler;

        _trackedModel = model;

        // Initial evaluation
        foreach(var cmd in commands)
            cmd.NotifyCanExecuteChanged();
    }

    // Shell Helpers...
    protected Task DisplayAlertAsync(string title, string message, string cancel) => ShellService.DisplayAlertAsync(title, message, cancel);

    protected Task<bool> DisplayConfirmAsync(string title, string message, string accept, string cancel) =>
        Shell.Current.DisplayAlertAsync(title, message, accept, cancel);

    protected async Task DisplayToastAsync(string message, ToastDuration toastDuration = ToastDuration.Short, double fontSize = 14) {
        var cancellationTokenSource = new CancellationTokenSource();
        var toast = Toast.Make(message, toastDuration, fontSize);
        await toast.Show(cancellationTokenSource.Token);
    }

    // Navigation helpers...
    protected Task NavigateAsync(string route) => ShellService.NavigateToAsync(route);
    protected Task NavigateAsync(string route, IDictionary<string, object> parameters) => ShellService.NavigateToAsync(route, parameters);
    protected Task NavigateBackAsync() => ShellService.NavigateBackAsync();
}