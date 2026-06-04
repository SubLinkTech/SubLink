using System;
using xyz.yewnyx.SubLink.Discord.Client;

namespace xyz.yewnyx.SubLink.Discord.Services;

internal sealed partial class DiscordService {
    public void Mute(bool mute) =>
        _discord?.SendCommand(1, DiscordIpcMessage.SetMuteOnly(mute));

    public void Deafen(bool deafen) =>
        _discord?.SendCommand(1, DiscordIpcMessage.SetDeafenOnly(deafen));

    public void RequestSelectedVoiceChannel() =>
        _discord?.SendCommand(1, DiscordIpcMessage.GetSelectedVoiceChannel());

    public void RequestVoiceSettings() =>
        _discord?.SendCommand(1, DiscordIpcMessage.GetVoiceSettings());

    public void SetInputVolume(float vol) =>
        _discord?.SendCommand(1, DiscordIpcMessage.SetVoiceSettings(new() {
            { "input", new { volume = vol } }
        }));

    public void SetOutputVolume(float vol) =>
        _discord?.SendCommand(1, DiscordIpcMessage.SetVoiceSettings(new() {
            { "output", new { volume = vol } }
        }));

    public void SubscribeEvent(string eventName, string? id = null) {
        object? args = null;

        if (!string.IsNullOrWhiteSpace(id)) {
            if (eventName.Equals("GUILD_STATUS", StringComparison.InvariantCultureIgnoreCase)) {
                args = new { guild_id = id };
            } else if (
                eventName.StartsWith("VOICE_STATE", StringComparison.InvariantCultureIgnoreCase) ||
                eventName.StartsWith("MESSAGE_", StringComparison.InvariantCultureIgnoreCase) ||
                eventName.Equals("SPEAKING_START", StringComparison.InvariantCultureIgnoreCase) ||
                eventName.Equals("SPEAKING_STOP", StringComparison.InvariantCultureIgnoreCase)
            ) {
                args = new { channel_id = id };
            }
        }

        _discord?.SendCommand(1, DiscordIpcMessage.Subscribe(eventName, args));
    }

    public void UnsubscribeEvent(string eventName, string? id = null) {
        object? args = null;

        if (!string.IsNullOrWhiteSpace(id)) {
            if (eventName.Equals("GUILD_STATUS", StringComparison.InvariantCultureIgnoreCase)) {
                args = new { guild_id = id };
            } else if (
                eventName.StartsWith("VOICE_STATE", StringComparison.InvariantCultureIgnoreCase) ||
                eventName.StartsWith("MESSAGE_", StringComparison.InvariantCultureIgnoreCase) ||
                eventName.Equals("SPEAKING_START", StringComparison.InvariantCultureIgnoreCase) ||
                eventName.Equals("SPEAKING_STOP", StringComparison.InvariantCultureIgnoreCase)
            ) {
                args = new { channel_id = id };
            }
        }

        _discord?.SendCommand(1, DiscordIpcMessage.Unsubscribe(eventName, args));
    }

    public void SelectVoiceChannel(string channelId, bool force) =>
        _discord?.SendCommand(1, DiscordIpcMessage.SelectVoiceChannel(channelId, force));

    public void SelectTextChannel(string channelId) =>
        _discord?.SendCommand(1, DiscordIpcMessage.SelectTextChannel(channelId));

    public void SetUserVolume(string userId, int vol) =>
        _discord?.SendCommand(1, DiscordIpcMessage.SetUserVoiceSettings(userId, volume: vol));

    public void SetUserMute(string userId, bool mute) =>
        _discord?.SendCommand(1, DiscordIpcMessage.SetUserVoiceSettings(userId, mute: mute));

    public void SetActivity(string state, string details) =>
        _discord?.SendCommand(1, DiscordIpcMessage.SetActivity(state, details, string.Empty, string.Empty));

    public void RequestGuildInfo(string guildId) =>
        _discord?.SendCommand(1, DiscordIpcMessage.GetGuild(guildId));
}
