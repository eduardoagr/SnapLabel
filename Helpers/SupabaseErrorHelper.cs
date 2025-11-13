

namespace SnapLabel.Helpers;
public static class SupabaseErrorHelper {

    public static async Task HandleAsync(Exception ex, IShellService shellService) {
        SupabaseErrorResponse? errorResponse;

        try {
            errorResponse = JsonSerializer.Deserialize<SupabaseErrorResponse>(ex.Message);
        } catch {

            await shellService.DisplayAlertAsync("Error", "An unexpected error occurred.", "OK");

            return;
        }

        var message = SupabaseErrorMessage.GetErrorMessage(errorResponse?.ErrorCode ?? "");

        await MainThread.InvokeOnMainThreadAsync(async () => {
            await shellService.DisplayAlertAsync("Error", message, "OK");
        });
    }
}
