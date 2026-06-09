using Microsoft.Extensions.Configuration;
using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick;

public sealed class JoystickSettings {
    [JsonPropertyName("Enabled"), ConfigurationKeyName("Enabled")]
    public bool Enabled { get; init; }

    [JsonPropertyName("OAuthPort"), ConfigurationKeyName("OAuthPort")]
    public ushort OAuthPort { get; init; }

    [JsonPropertyName("ApplicationId"), ConfigurationKeyName("ApplicationId")]
    public string ApplicationId { get; init; }

    [JsonPropertyName("ClientId"), ConfigurationKeyName("ClientId")]
    public string ClientId { get; init; }

    [JsonPropertyName("ClientSecret"), ConfigurationKeyName("ClientSecret")]
    public string ClientSecret { get; init; }

    [JsonPropertyName("AccessToken"), ConfigurationKeyName("AccessToken")]
    public string AccessToken { get; init; }

    [JsonPropertyName("RefreshToken"), ConfigurationKeyName("RefreshToken")]
    public string RefreshToken { get; init; }

    [JsonPropertyName("State"), ConfigurationKeyName("State")]
    public string State { get; init; }

    [JsonPropertyName("Username"), ConfigurationKeyName("Username")]
    public string Username { get; init; }

    [JsonPropertyName("ChannelId"), ConfigurationKeyName("ChannelId")]
    public string ChannelId { get; init; }

#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    public JoystickSettings() { }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    
    public JoystickSettings(bool enabled, ushort oauthPort, string applicationId, string clientId, string clientSecret,
        string accessToken, string refreshToken, string state, string username, string channelId) =>
        (Enabled, OAuthPort, ApplicationId, ClientId, ClientSecret, AccessToken, RefreshToken, State, Username, ChannelId) =
            (enabled, oauthPort, applicationId, clientId, clientSecret, accessToken, refreshToken, state, username, channelId);
}
