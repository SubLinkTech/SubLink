using System;

namespace xyz.yewnyx.SubLink.Discord.Client;

public sealed class DiscordErrorArgs(int code, string message) : EventArgs {
    public int Code { get; init; } = code;
    public string Message { get; init; } = message;
}

public sealed class DiscordVoiceSettingsEventArgs(
    float inputVolume, float outputVolume,
    string modeType, bool modeAutoThreshold, float modeThreshold, float modeDelay,
    bool autoGainControl, bool echoCancelation, bool noiseSurpression, bool qos,
    bool silenceWarning, bool deaf, bool mute
) : EventArgs {
    public float InputVolume { get; init; } = inputVolume;
    public float OutputVolume { get; init; } = outputVolume;
    public string ModeType { get; init; } = modeType;
    public bool ModeAutoThreshold { get; init; } = modeAutoThreshold;
    public float ModeThreshold { get; init; } = modeThreshold;
    public float ModeDelay { get; init; } = modeDelay;
    public bool AutoGainControl { get; init; } = autoGainControl;
    public bool EchoCancelation { get; init; } = echoCancelation;
    public bool NoiseSurpression { get; init; } = noiseSurpression;
    public bool Qos { get; init; } = qos;
    public bool SilenceWarning { get; init; } = silenceWarning;
    public bool Deaf { get; init; } = deaf;
    public bool Mute { get; init; } = mute;
}

public sealed class DiscordVoiceStatusEventArgs(string state, int stateCode) : EventArgs {
    public string State { get; init; } = state;
    public int StateCode { get; init; } = stateCode;
}

public sealed class DiscordGuildIdEventArgs(string guildId) : EventArgs {
    public string GuildId { get; init; } = guildId;
}

public class DiscordChannelIdEventArgs(string id) : EventArgs {
    public string Id { get; init; } = id;
}

public sealed class DiscordChannelEventArgs(string id, string name) : DiscordChannelIdEventArgs(id) {
    public string Name { get; init; } = name;
}

public sealed class DiscordVoiceChannelIdEventArgs(string vcId,  string guildId) : EventArgs {
    public string VoiceChannelId { get; init; } = vcId;
    public string GuildId { get; init; } = guildId;
}

public sealed class DiscordUserIdEventArgs(string userId) : EventArgs {
    public string UserId { get; init; } = userId;
}

public sealed class DiscordMessageIdEventArgs(string messageId) : EventArgs {
    public string MessageId { get; init; } = messageId;
}
