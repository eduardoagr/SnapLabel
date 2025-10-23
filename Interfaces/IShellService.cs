﻿namespace SnapLabel.Interfaces {

    public interface IShellService {

        Task DisplayAlertAsync(string title, string message, string cancel);

        Task DisplayToastAsync(string message, ToastDuration toastDuration = ToastDuration.Short,
            double fontSize = 14);

        Task NavigateBackAsync();

        Task NavigateToAsync(string route);
    }
}
