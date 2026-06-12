using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Message;

public sealed class NewMessageEvent {
    public class AuthorType {
        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

        [JsonPropertyName("username")]
        public string Username { get; set; } = string.Empty;

        [JsonPropertyName("usernameColor")]
        public string? UsernameColor { get; set; } = string.Empty;

        [JsonPropertyName("nickname")]
        public string? Nickname { get; set; } = string.Empty;

        [JsonPropertyName("isStreamer")]
        public bool IsStreamer { get; set; } = false;

        [JsonPropertyName("isModerator")]
        public bool IsModerator { get; set; } = false;

        [JsonPropertyName("isSubscriber")]
        public bool IsSubscriber { get; set; } = false;

        [JsonPropertyName("isVerified")]
        public bool IsVerified { get; set; } = false;

        [JsonPropertyName("isContentCreator")]
        public bool IsContentCreator { get; set; } = false;
    }

    public class StreamerType {
        [JsonPropertyName("slug")]
        public string Slug { get; set; } = string.Empty;

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

    [JsonPropertyName("botCommand")]
    public string? BotCommand { get; set; } = string.Empty;

    [JsonPropertyName("botCommandArg")]
    public string? BotCommandArg { get; set; } = string.Empty;

    [JsonPropertyName("emotesUsed")]
    public string[] EmotesUsed { get; set; } = []; // Undocumented type

    [JsonPropertyName("author")]
    public AuthorType Author { get; set; } = new();

    [JsonPropertyName("streamer")]
    public StreamerType Streamer { get; set; } = new();

    [JsonPropertyName("channelId")]
    public string ChannelId { get; set; } = string.Empty;

    [JsonPropertyName("mention")]
    public bool Mention { get; set; } = false;

    [JsonPropertyName("mentionedUsername")]
    public string? MentionedUsername { get; set; } = string.Empty;

    [JsonPropertyName("highlight")]
    public bool Highlight { get; set; } = false;
}
