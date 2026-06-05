using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Command;

public sealed class BlockUser : BaseCommand {
    public class ChatData {
        [JsonPropertyName("action")]
        public readonly string Action = "block_user";
        [JsonPropertyName("messageId")]
        public string MessageId { get; set; } = string.Empty;
        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; } = string.Empty;
    }

    public override string Command { get; } = "message";
    [JsonIgnore]
    public readonly ChatData Message = new();

    internal override string SerializeData() =>
        JsonSerializer.Serialize(Message, _options);
}
