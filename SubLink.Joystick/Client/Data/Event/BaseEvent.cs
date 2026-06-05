using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Event;

/*
Types:
- Started (event = StreamEvent)
- StreamResuming (event = StreamEvent)
- StreamEnding (event = StreamEvent)
- Ended (event = StreamEvent)

- Followed (event = StreamEvent)
- FollowerCountUpdated (event = StreamEvent)

- Tipped (event = StreamEvent)
- TipGoalCreated (event = StreamEvent)
- TipGoalDeleted (event = StreamEvent)
- TipGoalUpdated (event = StreamEvent)
- TipGoalIncreased (event = StreamEvent)
- TipGoalMet (event = StreamEvent)
- TipMenuItemLocked (event = StreamEvent)
- TipMenuItemUnlocked (event = StreamEvent)

- ChatTimerStarted (event = StreamEvent)
- ChatTimersCleared (event = StreamEvent)

- DropinStream (event = StreamEvent)
- StreamDroppedIn (event = StreamEvent)

- Subscribed (event = StreamEvent)
- Resubscribed (event = StreamEvent)
- GiftedSubscriptions (event = StreamEvent)

- WheelSpinClaimed (event = StreamEvent)
- ViewerCountUpdated (event = StreamEvent)
- SubscriberCountUpdated (event = StreamEvent)
- MilestoneCompleted (event = StreamEvent)

- PvpSessionRequested (event = StreamEvent)
- PvpSessionReady (event = StreamEvent)
- PvpSessionStarted (event = StreamEvent)
- PvpSessionEnding (event = StreamEvent)
- PvpSessionEnded (event = StreamEvent)

- SceneUpdated (event = StreamEvent)
- SettingsUpdated (event = StreamEvent)
- StreamModeUpdated (event = StreamEvent)

- UserMuted (event = StreamEvent)
- UserUnmuted (event = StreamEvent)

- DeviceConnected (event = StreamEvent)
- DeviceDisconnected (event = StreamEvent)
- DeviceSettingsUpdated (event = StreamEvent)

- ChatMessageReceived (event = StreamEvent)
- new_message (event = ChatMessage)
- event_bot_message (event = BotMessage)
- enter_stream (event = UserPresence)
*/
[JsonPolymorphic(
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor,
    TypeDiscriminatorPropertyName = "type"
)]
[JsonDerivedType(typeof(StartedEvent), "Started")]
public interface IBaseEvent {
    string Id { get; set; }
    string Event { get; set; }
    string Type { get; set; }
    string Text { get; set; }
    string ChannelId { get; set; }
    string CreatedAt { get; set; }
}

public abstract class  BaseEvent : IBaseEvent {
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;
    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
    [JsonPropertyName("channelId")]
    public string ChannelId { get; set; } = string.Empty;
    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;
}
