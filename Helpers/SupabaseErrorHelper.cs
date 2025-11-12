

namespace SnapLabel.Helpers;
public static class SupabaseErrorHelper {

    public static async Task HandleAsync(Exception ex, IShellService shellService) {
        SupabaseErrorResponse? errorResponse;

        try {
            errorResponse = JsonSerializer.Deserialize<SupabaseErrorResponse>(ex.Message);
        } catch {
            await shellService.DisplayAlertAsync("Error", ex.Message, "OK");
            return;
        }

        var message = SupabaseErrorMessage.GetErrorMessage(errorResponse?.ErrorCode ?? "");
        await shellService.DisplayAlertAsync("Error", message, "OK");
    }
}
