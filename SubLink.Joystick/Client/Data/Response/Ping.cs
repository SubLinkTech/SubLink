using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Response;

public sealed class Ping : IBaseResponse {
    [JsonPropertyName("message")]
    public long Message { get; set; } = 0;
}
