using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Message;

public sealed class BotMessageEvent {
    public class AuthorType {
        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("usernameColor")]
        public string? UsernameColor { get; set; } = string.Empty;
    }

    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    [JsonPropertyName("messageId")]
    public string MessageId { get; set; } = string.Empty;

    [JsonPropertyName("visibility")]
    public string Visibility { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("emotesUsed")]
    public string[] EmotesUsed { get; set; } = []; // Undocumented type

    [JsonPropertyName("author")]
    public AuthorType Author { get; set; } = new();

    [JsonPropertyName("mention")]
    public bool Mention { get; set; } = false;
}
