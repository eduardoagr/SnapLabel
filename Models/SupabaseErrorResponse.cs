namespace SnapLabel.Models;

public class SupabaseErrorResponse {

    [JsonPropertyName("error_code")]
    public string? ErrorCode { get; set; }

    [JsonPropertyName("msg")]
    public string? Msg { get; set; }

}
