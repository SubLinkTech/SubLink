using System;

namespace xyz.yewnyx.SubLink.Discord.Client;

public sealed class DiscordErrorArgs(int code, string message) : EventArgs {
    public int Code { get; init; } = code;
    public string Message { get; init; } = message;
}

public sealed class DiscordGuildInfoEventArgs(string guildId, string guildName, string iconUrl) : EventArgs {
    public string Id { get; init; } = guildId;
    public string Name { get; init; } = guildName;
    public string IconUrl { get; init; } = iconUrl;
}

public sealed class DiscordChannelCreateEventArgs(string id, string name, ChannelType type) : EventArgs {
    public string Id { get; init; } = id;
    public string Name { get; init; } = name;
    public ChannelType Type { get; init; } = type;
}

public sealed class DiscordVoiceChannelIdEventArgs(string vcId,  string guildId) : EventArgs {
    public string ChannelId { get; init; } = vcId;
    public string GuildId { get; init; } = guildId;
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

public sealed class DiscordVoiceStateEventArgs(
    string userId, string userName, string userNickname, bool isBot, int userVolume, bool userMute,
    bool stateMute, bool stateDeaf, bool stateSelfMute, bool stateSelfDeaf, bool stateSuppress
) : EventArgs {
    public string Id { get; init; } = userId;
    public string Username { get; init; } = userName;
    public string Nickname { get; init; } = userNickname;
    public bool IsBot { get; init; } = isBot;
    public int Volume { get; init; } = userVolume;
    public bool Muted { get; init; } = userMute;
    public bool StateMuted { get; init; } = stateMute;
    public bool StateDeadened { get; init; } = stateDeaf;
    public bool StateSelfMuted { get; init; } = stateSelfMute;
    public bool StateSelfDeadened { get; init; } = stateSelfDeaf;
    public bool StateSuppressed { get; init; } = stateSuppress;
}

public sealed class DiscordVoiceStatusEventArgs(string state, int stateCode) : EventArgs {
    public string State { get; init; } = state;
    public int StateCode { get; init; } = stateCode;
}

public sealed class DiscordMessageEventArgs(
    string channelId, string messageId, bool messageBlocked, string messageContent, string messageTimestamp,
    string messageEditedTimestamp, bool messagePinned, string messageAuthorId, string messageAuthorUsername,
    bool messageAuthorIsBot
) : EventArgs {
    public string ChannelId { get; init; } = channelId;
    public string MessageId { get; init; } = messageId;
    public bool IsBlocked { get; init; } = messageBlocked;
    public string Content { get; init; } = messageContent;
    public string Timestamp { get; init; } = messageTimestamp;
    public string EditedTimestamp { get; init; } = messageEditedTimestamp;
    public bool IsPinned { get; init; } = messagePinned;
    public string UserId { get; init; } = messageAuthorId;
    public string Username { get; init; } = messageAuthorUsername;
    public bool IsBot { get; init; } = messageAuthorIsBot;
}

public sealed class DiscordUserIdEventArgs(string userId) : EventArgs {
    public string UserId { get; init; } = userId;
}

public sealed class DiscordNotificationEventArgs(
    string channelId, string title, string body, string iconUrl, string messageId, string messageContent,
    string messageTimestamp, bool messagePinned, string messageAuthorId, string messageAuthorUsername,
    bool messageAuthorIsBot
) : EventArgs {
    public string ChannelId { get; init; } = channelId;
    public string Title { get; init; } = title;
    public string Body { get; init; } = body;
    public string IconUrl { get; init; } = iconUrl;
    public DiscordMessageEventArgs Message = new(
        channelId, messageId, false, messageContent, messageTimestamp, messageTimestamp,
        messagePinned, messageAuthorId, messageAuthorUsername, messageAuthorIsBot
    );
}
