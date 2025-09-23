namespace SnapLabel.Controls {
    public partial class InfiniteProgressBar : ProgressBar {

        private CancellationTokenSource? _cts;

        public InfiniteProgressBar() {

            Loaded += InfiniteProgressBar_Loaded;

            Unloaded += InfiniteProgressBar_Unloaded;
        }

        private void InfiniteProgressBar_Loaded(object? sender, EventArgs e) {

            _cts = new CancellationTokenSource();
            Progress = 0;
            StartAnimationLoop(_cts.Token);
        }

        private async void StartAnimationLoop(CancellationToken token) {
            while(!token.IsCancellationRequested) {

                try {

                    // Start from 0 each time before animation
                    Progress = 0;

                    await ProgressTo(1, 5000, Easing.Linear);

                    Progress = 0;

                } catch(TaskCanceledException) {

                    break;
                }
            }
        }

        private void InfiniteProgressBar_Unloaded(object? sender, EventArgs e) {
            _cts!.Cancel();
        }


    }
}
