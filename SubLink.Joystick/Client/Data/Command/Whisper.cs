using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Command;

public sealed class Whisper : BaseCommand {
    public class ChatData {
        [JsonPropertyName("action")]
        public readonly string Action = "send_whisper";
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;
        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;
        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; } = string.Empty;
    }

    public override string Command { get; } = "message";
    [JsonIgnore]
    public readonly ChatData Message = new();

    internal override string SerializeData() =>
        JsonSerializer.Serialize(Message, _options);
}
