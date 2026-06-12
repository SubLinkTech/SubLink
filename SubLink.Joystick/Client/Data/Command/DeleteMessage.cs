using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Command;

public sealed class DeleteMessage : BaseCommand {
    public class ChatData {
        [JsonPropertyName("action")]
        public readonly string Action = "delete_message";

        [JsonPropertyName("messageId")]
        public string MessageId { get; set; } = string.Empty;

        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; } = string.Empty;
    }

    [JsonIgnore]
    public readonly ChatData Message = new();

    internal override string SerializeData() =>
        JsonSerializer.Serialize(Message, _options);

    public DeleteMessage() { }

    public DeleteMessage(string messageId, string channelId) =>
        (Message.MessageId, Message.ChannelId) = (messageId, channelId);
}
