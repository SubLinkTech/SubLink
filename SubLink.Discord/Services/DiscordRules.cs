using JetBrains.Annotations;
using System;
using System.Threading.Tasks;
using xyz.yewnyx.SubLink.Discord.Client;
using xyz.yewnyx.SubLink.Platforms;

namespace xyz.yewnyx.SubLink.Discord.Services;

[PublicAPI]
public sealed class DiscordRules : IPlatformRules {
    private DiscordService? _service;

    internal void SetService(DiscordService service) {
        _service = service;
    }

    internal Func<Task>? OnReady;
    internal Func<DiscordErrorArgs, Task>? OnError;
    internal Func<DiscordGuildInfoEventArgs, Task>? OnGuildStatus;
    internal Func<DiscordGuildInfoEventArgs, Task>? OnGuildCreate;
    internal Func<DiscordChannelCreateEventArgs, Task>? OnChannelCreate;
    internal Func<DiscordVoiceChannelIdEventArgs, Task>? OnSelectedVoiceChannel;
    internal Func<DiscordVoiceSettingsEventArgs, Task>? OnVoiceSettingsUpdate;
    internal Func<DiscordVoiceStateEventArgs, Task>? OnVoiceStateCreate;
    internal Func<DiscordVoiceStateEventArgs, Task>? OnVoiceStateUpdate;
    internal Func<DiscordVoiceStateEventArgs, Task>? OnVoiceStateDelete;
    internal Func<DiscordVoiceStatusEventArgs, Task>? OnVoiceStatusUpdate;
    internal Func<DiscordMessageEventArgs, Task>? OnMessageCreate;
    internal Func<DiscordMessageEventArgs, Task>? OnMessageUpdate;
    internal Func<DiscordMessageEventArgs, Task>? OnMessageDelete;
    internal Func<string, Task>? OnStartSpeaking;
    internal Func<string, Task>? OnStopSpeaking;
    internal Func<DiscordNotificationEventArgs, Task>? OnNotificationCreate;
    internal Func<Task>? OnActivityJoin;
    internal Func<Task>? OnActivitySpectate;
    internal Func<string, Task>? OnActivityJoinRequest;

    /* Reacts */
    public void ReactToReady(Func<Task> with) { OnReady = with; }
    public void ReactToError(Func<DiscordErrorArgs, Task> with) { OnError = with; }
    public void ReactToGuildStatus(Func<DiscordGuildInfoEventArgs, Task> with) { OnGuildStatus = with; }
    public void ReactToGuildCreate(Func<DiscordGuildInfoEventArgs, Task> with) { OnGuildCreate = with; }
    public void ReactToChannelCreate(Func<DiscordChannelCreateEventArgs, Task> with) { OnChannelCreate = with; }
    public void ReactToSelectedVoiceChannel(Func<DiscordVoiceChannelIdEventArgs, Task> with) { OnSelectedVoiceChannel = with; }
    public void ReactToVoiceSettingsUpdate(Func<DiscordVoiceSettingsEventArgs, Task> with) { OnVoiceSettingsUpdate = with; }
    public void ReactToVoiceStateCreate(Func<DiscordVoiceStateEventArgs, Task> with) { OnVoiceStateCreate = with; }
    public void ReactToVoiceStateUpdate(Func<DiscordVoiceStateEventArgs, Task> with) { OnVoiceStateUpdate = with; }
    public void ReactToVoiceStateDelete(Func<DiscordVoiceStateEventArgs, Task> with) { OnVoiceStateDelete = with; }
    public void ReactToVoiceStatusUpdate(Func<DiscordVoiceStatusEventArgs, Task> with) { OnVoiceStatusUpdate = with; }
    public void ReactToMessageCreate(Func<DiscordMessageEventArgs, Task> with) { OnMessageCreate = with; }
    public void ReactToMessageUpdate(Func<DiscordMessageEventArgs, Task> with) { OnMessageUpdate = with; }
    public void ReactToMessageDelete(Func<DiscordMessageEventArgs, Task> with) { OnMessageDelete = with; }
    public void ReactToStartSpeaking(Func<string, Task> with) { OnStartSpeaking = with; }
    public void ReactToStopSpeaking(Func<string, Task> with) { OnStopSpeaking = with; }
    public void ReactToNotificationCreate(Func<DiscordNotificationEventArgs, Task> with) { OnNotificationCreate = with; }
    public void ReactToActivityJoin(Func<Task> with) { OnActivityJoin = with; }
    public void ReactToActivitySpectate(Func<Task> with) { OnActivitySpectate = with; }
    public void ReactToActivityJoinRequest(Func<string, Task> with) { OnActivityJoinRequest = with; }

    /* Actions */
    public void Mute() =>
        _service?.Mute(true);

    public void Unmute() =>
        _service?.Mute(false);

    public void Deafen() =>
        _service?.Deafen(true);

    public void Undeafen() =>
        _service?.Deafen(false);

    public void RequestSelectedVoiceChannel() =>
        _service?.RequestSelectedVoiceChannel();

    public void RequestVoiceSettings() =>
        _service?.RequestVoiceSettings();

    public void SetInputVolume(float vol) =>
        _service?.SetInputVolume(vol);

    public void SetOutputVolume(float vol) =>
        _service?.SetOutputVolume(vol);

    public void SubscribeEvent(string eventName, string? id = null) =>
        _service?.SubscribeEvent(eventName, id);

    public void UnsubscribeEvent(string eventName, string? id = null) =>
        _service?.UnsubscribeEvent(eventName, id);

    public void SelectVoiceChannel(string channelId, bool force = true) =>
        _service?.SelectVoiceChannel(channelId, force);

    public void SelectTextChannel(string channelId) =>
        _service?.SelectTextChannel(channelId);

    public void SetUserVolume(string userId, int vol) =>
        _service?.SetUserVolume(userId, vol);

    public void SetUserVolume(string userId, float vol) =>
        _service?.SetUserVolume(userId, Convert.ToInt32(Math.Round(vol)));

    public void MuteUser(string userId) =>
        _service?.SetUserMute(userId, true);

    public void UnmuteUser(string userId) =>
        _service?.SetUserMute(userId, false);

    public void SetActivity(string state, string details) =>
        _service?.SetActivity(state, details);
}
