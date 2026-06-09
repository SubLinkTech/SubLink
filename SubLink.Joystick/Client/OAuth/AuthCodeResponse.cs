using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.OAuth;

public class AuthCodeResponse {
    [JsonPropertyName("access_token"), JsonRequired]
    public string AccessToken { get; set; } = string.Empty;

    [JsonPropertyName("token_type"), JsonRequired]
    public string TokenType { get; set; } = string.Empty;

    [JsonPropertyName("expires_in"), JsonRequired]
    public int ExpiresIn { get; set; } = -1;

    [JsonPropertyName("refresh_token"), JsonRequired]
    public string RefreshToken { get; set; } = string.Empty;
}
