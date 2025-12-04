namespace SnapLabel.Helpers;

public class Validation {

    /// <summary>
    /// Returns true if all provided strings are non-null, non-empty, and not just whitespace.
    /// </summary>

    public static bool AllFilled(params string?[] fields) => fields.All(f => !string.IsNullOrWhiteSpace(f));

    public static bool AllFilled(params byte[][] arrays) => arrays.All(a => a is { Length: > 0 });

}
