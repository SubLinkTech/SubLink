using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Api;

public sealed class StreamSettings {
    [JsonPropertyName("username")]
    public string Username { get; set; } = string.Empty;
    [JsonPropertyName("stream_title")]
    public string StreamTitle { get; set; } = string.Empty;
    [JsonPropertyName("chat_welcome_message")]
    public string ChatWelcomeMessage { get; set; } = string.Empty;
    [JsonPropertyName("banned_chat_words")]
    public string[] BannedChatWords { get; set; } = [];
    [JsonPropertyName("device_active")]
    public bool DeviceActive { get; set; } = false;
    [JsonPropertyName("photo_url")]
    public string PhotoUrl { get; set; } = string.Empty;
    [JsonPropertyName("live")]
    public bool Live { get; set; } = false;
    [JsonPropertyName("number_of_followers")]
    public int NumberOfFollowers { get; set; } = 0;
    [JsonPropertyName("channel_id")]
    public string ChannelId { get; set; } = string.Empty;
}
