namespace SnapLabel.Helpers;

public class CredentialVault {

    private const string Passphrase = "Secrets";

    public static async Task StorePasswordAsync(string password) {
        var encrypted = AES.Encrypt(password, Passphrase);
        await SecureStorage.SetAsync(AppConstants.PASSWORD, encrypted);
    }

    public static async Task<string?> RetrievePasswordAsync() {
        var decrypted = await SecureStorage.GetAsync(AppConstants.PASSWORD);
        if(string.IsNullOrEmpty(decrypted))
            return null;

        return AES.Decrypt(decrypted, Passphrase);
    }

    public static void ClearPassword() {
        SecureStorage.Remove("password");
    }

}
