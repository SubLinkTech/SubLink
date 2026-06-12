using System.Text.Json;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Command;

public sealed class MuteUser : BaseCommand {
    public class ChatData {
        [JsonPropertyName("action")]
        public readonly string Action = "mute_user";

        [JsonPropertyName("messageId")]
        public string MessageId { get; set; } = string.Empty;

        [JsonPropertyName("channelId")]
        public string ChannelId { get; set; } = string.Empty;
    }

    [JsonIgnore]
    public readonly ChatData Message = new();

    internal override string SerializeData() =>
        JsonSerializer.Serialize(Message, _options);

    public MuteUser() { }

    public MuteUser(string messageId, string channelId) =>
        (Message.MessageId, Message.ChannelId) = (messageId, channelId);
}
