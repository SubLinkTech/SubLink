using System.Text.Json.Serialization;

namespace xyz.yewnyx.SubLink.Joystick.Client.Data.Event;

[JsonPolymorphic(
    UnknownDerivedTypeHandling = JsonUnknownDerivedTypeHandling.FallBackToNearestAncestor,
    TypeDiscriminatorPropertyName = "type"
)]
[JsonDerivedType(typeof(EnterStreamEvent), "enter_stream")]
[JsonDerivedType(typeof(LeaveStreamEvent), "leave_stream")]
[JsonDerivedType(typeof(StartedEvent), "Started")]
[JsonDerivedType(typeof(StreamResumingEvent), "StreamResuming")]
[JsonDerivedType(typeof(StreamEndingEvent), "StreamEnding")]
[JsonDerivedType(typeof(EndedEvent), "Ended")]
[JsonDerivedType(typeof(FollowedEvent), "Followed")]
[JsonDerivedType(typeof(FollowerCountUpdatedEvent), "FollowerCountUpdated")]
[JsonDerivedType(typeof(TippedEvent), "Tipped")]
[JsonDerivedType(typeof(TipGoalCreatedEvent), "TipGoalCreated")]
[JsonDerivedType(typeof(TipGoalDeletedEvent), "TipGoalDeleted")]
[JsonDerivedType(typeof(TipGoalUpdatedEvent), "TipGoalUpdated")]
[JsonDerivedType(typeof(TipGoalIncreasedEvent), "TipGoalIncreased")]
[JsonDerivedType(typeof(TipGoalMetEvent), "TipGoalMet")]
[JsonDerivedType(typeof(TipMenuItemLockedEvent), "TipMenuItemLocked")]
[JsonDerivedType(typeof(TipMenuItemUnlockedEvent), "TipMenuItemUnlocked")]
[JsonDerivedType(typeof(ChatTimerStartedEvent), "ChatTimerStarted")]
[JsonDerivedType(typeof(ChatTimersClearedEvent), "ChatTimersCleared")]
[JsonDerivedType(typeof(DropinStreamEvent), "DropinStream")]
[JsonDerivedType(typeof(StreamDroppedInEvent), "StreamDroppedIn")]
[JsonDerivedType(typeof(SubscribedEvent), "Subscribed")]
[JsonDerivedType(typeof(ResubscribedEvent), "Resubscribed")]
[JsonDerivedType(typeof(GiftedSubscriptionsEvent), "GiftedSubscriptions")]
[JsonDerivedType(typeof(WheelSpinClaimedEvent), "WheelSpinClaimed")]
[JsonDerivedType(typeof(ViewerCountUpdatedEvent), "ViewerCountUpdated")]
[JsonDerivedType(typeof(SubscriberCountUpdatedEvent), "SubscriberCountUpdated")]
[JsonDerivedType(typeof(MilestoneCompletedEvent), "MilestoneCompleted")]
[JsonDerivedType(typeof(PvpSessionRequestedEvent), "PvpSessionRequested")]
[JsonDerivedType(typeof(PvpSessionReadyEvent), "PvpSessionReady")]
[JsonDerivedType(typeof(PvpSessionStartedEvent), "PvpSessionStarted")]
[JsonDerivedType(typeof(PvpSessionEndingEvent), "PvpSessionEnding")]
[JsonDerivedType(typeof(PvpSessionEndedEvent), "PvpSessionEnded")]
[JsonDerivedType(typeof(SceneUpdatedEvent), "SceneUpdated")]
[JsonDerivedType(typeof(SettingsUpdatedEvent), "SettingsUpdated")]
[JsonDerivedType(typeof(StreamModeUpdatedEvent), "StreamModeUpdated")]
[JsonDerivedType(typeof(UserMutedEvent), "UserMuted")]
[JsonDerivedType(typeof(UserUnmutedEvent), "UserUnmuted")]
[JsonDerivedType(typeof(DeviceConnectedEvent), "DeviceConnected")]
[JsonDerivedType(typeof(DeviceDisconnectedEvent), "DeviceDisconnected")]
[JsonDerivedType(typeof(DeviceSettingsUpdatedEvent), "DeviceSettingsUpdated")]
public interface IBaseEvent { }

public abstract class  BaseMessage : IBaseEvent {
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("event")]
    public string Event { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;

    [JsonPropertyName("channelId")]
    public string ChannelId { get; set; } = string.Empty;

    [JsonPropertyName("createdAt")]
    public string CreatedAt { get; set; } = string.Empty;

    internal string _metadataStr = string.Empty;

    [JsonPropertyName("metadata")]
    public string JsonMetaObj {
        get {
            GetMetadata();
            return _metadataStr;
        }
        set {
            _metadataStr = value;
            SetMetadata();
        }
    }

    internal virtual void SetMetadata() { }
    internal virtual void GetMetadata() { }
}
