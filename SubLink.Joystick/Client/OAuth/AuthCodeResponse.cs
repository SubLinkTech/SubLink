using Newtonsoft.Json;

namespace xyz.yewnyx.SubLink.Joystick.Client.OAuth;

public class AuthCodeResponse {
    [JsonProperty("access_token")]
    public string AccessToken { get; protected set; } = string.Empty;

    [JsonProperty("token_type")]
    public string TokenType { get; protected set; } = string.Empty;

    [JsonProperty(PropertyName = "expires_in")]
    public int ExpiresIn { get; protected set; } = -1;

    [JsonProperty(PropertyName = "refresh_token")]
    public string RefreshToken { get; protected set; } = string.Empty;
}
