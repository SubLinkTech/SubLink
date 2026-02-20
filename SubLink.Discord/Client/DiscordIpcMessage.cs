using System;
using System.Collections.Generic;

namespace xyz.yewnyx.SubLink.Discord.Client;

internal static class DiscordIpcMessage {
    public static object Handshake(string clientId) =>
        new { v = 1, client_id = clientId };

    public static object Authenticate(string accessToken) =>
        new {
            cmd = "AUTHENTICATE",
            args = new { access_token = accessToken },
            nonce = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };

    /// <summary>
    /// Minimal payload for muting only
    /// </summary>
    /// <param name="mute"></param>
    /// <returns></returns>
    public static object SetMuteOnly(bool mute) =>
        new {
            cmd = "SET_VOICE_SETTINGS",
            args = new { mute },
            nonce = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };

    public static object GetVoiceSettings() =>
        new {
            cmd = "GET_VOICE_SETTINGS",
            args = new { },
            nonce = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };

    public static object SetActivity(string state, string details, string largeImageKey, string smallImageKey) =>
        new {
            cmd = "SET_ACTIVITY",
            args = new {
                pid = Environment.ProcessId,
                activity = new {
                    state,
                    details,
                    assets = new {
                        large_image = largeImageKey,
                        small_image = smallImageKey
                    }
                }
            },
            nonce = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };

    public static object GetGuild(string guildId) =>
        new {
            cmd = "GET_GUILD",
            args = new { guild_id = guildId },
            nonce = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };

    public static object SelectVoiceChannel(string channelId, bool indForce) =>
        new {
            cmd = "SELECT_VOICE_CHANNEL",
            args = new { channel_id = channelId, force = indForce },
            nonce = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };

    public static object Subscribe(string eventName) =>
        Subscribe(eventName, null);

    public static object Subscribe(string eventName, object? args) =>
        new {
            cmd = "SUBSCRIBE",
            args = args ?? new { },
            evt = eventName,
            nonce = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };

    public static object Unsubscribe(string eventName, object? args) =>
        new {
            cmd = "UNSUBSCRIBE",
            args = args ?? new { },
            evt = eventName,
            nonce = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };

    public static object SetDeafenOnly(bool deafen) =>
        new {
            cmd = "SET_VOICE_SETTINGS",
            args = new { deaf = deafen },
            nonce = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };

    public static object GetSelectedVoiceChannel() =>
        new {
            cmd = "GET_SELECTED_VOICE_CHANNEL",
            args = new { },
            nonce = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };

    public static object SelectTextChannel(string channelId) =>
        new {
            cmd = "SELECT_TEXT_CHANNEL",
            args = new { channel_id = channelId },
            nonce = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };

    public static object SetUserVoiceSettings(string userId, float? left = null, float? right = null, int? volume = null, bool? mute = null) {
        object? pan = null;

        if (left.HasValue && right.HasValue)
            pan = new { left = left.Value, right = right.Value };

        return new {
            cmd = "SET_USER_VOICE_SETTINGS",
            args = new {
                user_id = userId,
                pan,
                volume,
                mute
            },
            nonce = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };
    }

    public static object SetVoiceSettings(Dictionary<string, object> args) =>
        new {
            cmd = "SET_VOICE_SETTINGS",
            args,
            nonce = $"{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()}"
        };
}
