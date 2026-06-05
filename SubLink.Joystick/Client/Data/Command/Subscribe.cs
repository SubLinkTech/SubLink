using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Command;

public sealed class Subscribe : IBaseCommand {
    [JsonPropertyName("command")]
    public string Command { get; } = "subscribe";
    [JsonPropertyName("identifier")]
    public string Identifier { get; } = "{\"channel\":\"GatewayChannel\"}";
}
