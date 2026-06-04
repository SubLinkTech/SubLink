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
    internal Func<string, Task>? OnSelectedVoiceChannel;
    internal Func<DiscordVoiceSettingsEventArgs, Task>? OnVoiceSettingsUpdate;
    internal Func<DiscordVoiceStatusEventArgs, Task>? OnVoiceStatusUpdate;
    internal Func<string, Task>? OnGuildStatus;
    internal Func<string, Task>? OnGuildCreate;
    internal Func<DiscordChannelEventArgs, Task>? OnChannelCreate;
    internal Func<string, Task>? OnVoiceStateCreate;
    internal Func<string, Task>? OnVoiceStateUpdate;
    internal Func<string, Task>? OnVoiceStateDelete;
    internal Func<string, Task>? OnStartSpeaking;
    internal Func<string, Task>? OnStopSpeaking;
    internal Func<string, Task>? OnMessageCreate;
    internal Func<string, Task>? OnMessageUpdate;
    internal Func<string, Task>? OnMessageDelete;
    internal Func<string, Task>? OnNotificationCreate;
    internal Func<Task>? OnActivityJoin;
    internal Func<Task>? OnActivitySpectate;
    internal Func<string, Task>? OnActivityJoinRequest;

    /* Reacts */
    public void ReactToReady(Func<Task> with) { OnReady = with; }
    public void ReactToError(Func<DiscordErrorArgs, Task> with) { OnError = with; }
    public void ReactToSelectedVoiceChannel(Func<string, Task> with) { OnSelectedVoiceChannel = with; }
    public void ReactToVoiceSettingsUpdate(Func<DiscordVoiceSettingsEventArgs, Task> with) { OnVoiceSettingsUpdate = with; }
    public void ReactToVoiceStatusUpdate(Func<DiscordVoiceStatusEventArgs, Task> with) { OnVoiceStatusUpdate = with; }
    public void ReactToGuildStatus(Func<string, Task> with) { OnGuildStatus = with; }
    public void ReactToGuildCreate(Func<string, Task> with) { OnGuildCreate = with; }
    public void ReactToChannelCreate(Func<DiscordChannelEventArgs, Task> with) { OnChannelCreate = with; }
    public void ReactToVoiceStateCreate(Func<string, Task> with) { OnVoiceStateCreate = with; }
    public void ReactToVoiceStateUpdate(Func<string, Task> with) { OnVoiceStateUpdate = with; }
    public void ReactToVoiceStateDelete(Func<string, Task> with) { OnVoiceStateDelete = with; }
    public void ReactToStartSpeaking(Func<string, Task> with) { OnStartSpeaking = with; }
    public void ReactToStopSpeaking(Func<string, Task> with) { OnStopSpeaking = with; }
    public void ReactToMessageCreate(Func<string, Task> with) { OnMessageCreate = with; }
    public void ReactToMessageUpdate(Func<string, Task> with) { OnMessageUpdate = with; }
    public void ReactToMessageDelete(Func<string, Task> with) { OnMessageDelete = with; }
    public void ReactToNotificationCreate(Func<string, Task> with) { OnNotificationCreate = with; }
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
