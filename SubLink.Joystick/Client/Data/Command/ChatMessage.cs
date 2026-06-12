using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Command;

public sealed class ChatMessage : BaseCommand {
    public class ChatData {
        [JsonPropertyName("action")]
        public readonly string Action = "send_message";

        [JsonPropertyName("text")]
        public string Text { get; set; } = string.Empty;

        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; } = string.Empty;
    }

    [JsonIgnore]
    public readonly ChatData Message = new();

    internal override string SerializeData() =>
        JsonSerializer.Serialize(Message, _options);

    public ChatMessage() { }

    public ChatMessage(string text, string channelId) =>
        (Message.Text, Message.ChannelId) = (text, channelId);
}
