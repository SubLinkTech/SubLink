using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Command;

public sealed class UnmuteUser : BaseCommand {
    public class ChatData {
        [JsonIgnore]
        public string StreamerName { get; set; } = string.Empty;
        [JsonPropertyName("action")]
        public readonly string Action = "unmute_user";
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; } = string.Empty;
    }

    public override string Command { get; } = "message";
    [JsonIgnore]
    public readonly ChatData Message = new();

    internal override string SerializeIdent =>
        $"{{\"channel\":\"GatewayChannel\",\"streamer\":\"{Message.StreamerName}\"}}";

    internal override string SerializeData() =>
        JsonSerializer.Serialize(Message, _options);
}
