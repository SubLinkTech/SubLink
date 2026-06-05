using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Response;

public sealed class Ping : BaseResponse {
    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
