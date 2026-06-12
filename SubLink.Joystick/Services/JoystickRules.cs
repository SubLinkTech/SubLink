using JetBrains.Annotations;
using System;
using System.Threading.Tasks;
using xyz.yewnyx.SubLink.Joystick.Client;
using xyz.yewnyx.SubLink.Platforms;

namespace xyz.yewnyx.SubLink.Joystick.Services;

[PublicAPI]
public sealed class JoystickRules : IPlatformRules {
    private JoystickService? _service;

    internal Func<JoystickChatMessageEventArgs, Task>? OnJoystickChatMessage;
    internal Func<JoystickBotMessageEventArgs, Task>? OnJoystickBotMessage;
    internal Func<JoystickEnterStreamEventArgs, Task>? OnJoystickEnterStream;
    internal Func<JoystickLeaveStreamEventArgs, Task>? OnJoystickLeaveStream;
    internal Func<JoystickWhoEventArgs, Task>? OnJoystickStarted;
    internal Func<JoystickWhoEventArgs, Task>? OnJoystickStreamResuming;
    internal Func<JoystickWhoEventArgs, Task>? OnJoystickStreamEnding;
    internal Func<JoystickWhoEventArgs, Task>? OnJoystickEnded;
    internal Func<JoystickWhoWhatEventArgs, Task>? OnJoystickFollowed;
    internal Func<JoystickFollowerCountUpdatedEventArgs, Task>? OnJoystickFollowerCountUpdated;
    internal Func<JoystickTippedEventArgs, Task>? OnJoystickTipped;
    internal Func<JoystickTitleAmountEventArgs, Task>? OnJoystickTipGoalCreated;
    internal Func<JoystickTitleAmountEventArgs, Task>? OnJoystickTipGoalDeleted;
    internal Func<JoystickTipGoalIncreasedEventArgs, Task>? OnJoystickTipGoalIncreased;
    internal Func<JoystickWhoWhatTitleAmountEventArgs, Task>? OnJoystickTipGoalMet;
    internal Func<JoystickTitleAmountEventArgs, Task>? OnJoystickTipGoalUpdated;
    internal Func<JoystickTitleAmountEventArgs, Task>? OnJoystickTipMenuItemLocked;
    internal Func<JoystickTitleAmountEventArgs, Task>? OnJoystickTipMenuItemUnlocked;
    internal Func<JoystickChatTimerStartedEventArgs, Task>? OnJoystickChatTimerStarted;
    internal Func<JoystickWhoEventArgs, Task>? OnJoystickChatTimersCleared;
    internal Func<JoystickDropinStreamEventArgs, Task>? OnJoystickDropinStream;
    internal Func<JoystickStreamDroppedInEventArgs, Task>? OnJoystickStreamDroppedIn;
    internal Func<JoystickWhoWhatAmountEventArgs, Task>? OnJoystickSubscribed;
    internal Func<JoystickResubscribedEventArgs, Task>? OnJoystickResubscribed;
    internal Func<JoystickWhoWhatAmountEventArgs, Task>? OnJoystickGiftedSubscriptions;
    internal Func<JoystickWheelSpinClaimedEventArgs, Task>? OnJoystickWheelSpinClaimed;
    internal Func<JoystickAmountEventArgs, Task>? OnJoystickViewerCountUpdated;
    internal Func<JoystickAmountEventArgs, Task>? OnJoystickSubscriberCountUpdated;
    internal Func<JoystickWhoWhatTitleAmountEventArgs, Task>? OnJoystickMilestoneCompleted;
    internal Func<JoystickPvpSessionRequestedEventArgs, Task>? OnJoystickPvpSessionRequested;
    internal Func<JoystickPvpSessionReadyEventArgs, Task>? OnJoystickPvpSessionReady;
    internal Func<JoystickPvpSessionStartedEventArgs, Task>? OnJoystickPvpSessionStarted;
    internal Func<JoystickWhoWhatWhenWhereEventArgs, Task>? OnJoystickPvpSessionEnding;
    internal Func<JoystickWhoWhatWhenWhereEventArgs, Task>? OnJoystickPvpSessionEnded;
    internal Func<JoystickSceneUpdatedEventArgs, Task>? OnJoystickSceneUpdated;
    internal Func<JoystickEventArgs, Task>? OnJoystickSettingsUpdated;
    internal Func<JoystickWhoEventArgs, Task>? OnJoystickStreamModeUpdated;
    internal Func<JoystickWhoWhatEventArgs, Task>? OnJoystickUserMuted;
    internal Func<JoystickWhoWhatEventArgs, Task>? OnJoystickUserUnmuted;
    internal Func<JoystickEventArgs, Task>? OnJoystickDeviceConnected;
    internal Func<JoystickEventArgs, Task>? OnJoystickDeviceDisconnected;
    internal Func<JoystickEventArgs, Task>? OnJoystickDeviceSettingsUpdated;

    internal void SetService(JoystickService service) {
        _service = service;
    }

    /* Reacts */
    public void ReactToChatMessage(Func<JoystickChatMessageEventArgs, Task> with) { OnJoystickChatMessage = with; }
    public void ReactToBotMessage(Func<JoystickBotMessageEventArgs, Task> with) { OnJoystickBotMessage = with; }
    public void ReactToEnterStream(Func<JoystickEnterStreamEventArgs, Task> with) { OnJoystickEnterStream = with; }
    public void ReactToLeaveStream(Func<JoystickLeaveStreamEventArgs, Task> with) { OnJoystickLeaveStream = with; }
    public void ReactToStarted(Func<JoystickWhoEventArgs, Task> with) { OnJoystickStarted = with; }
    public void ReactToStreamResuming(Func<JoystickWhoEventArgs, Task> with) { OnJoystickStreamResuming = with; }
    public void ReactToStreamEnding(Func<JoystickWhoEventArgs, Task> with) { OnJoystickStreamEnding = with; }
    public void ReactToEnded(Func<JoystickWhoEventArgs, Task> with) { OnJoystickEnded = with; }
    public void ReactToFollowed(Func<JoystickWhoWhatEventArgs, Task> with) { OnJoystickFollowed = with; }
    public void ReactToFollowerCountUpdated(Func<JoystickFollowerCountUpdatedEventArgs, Task> with) { OnJoystickFollowerCountUpdated = with; }
    public void ReactToTipped(Func<JoystickTippedEventArgs, Task> with) { OnJoystickTipped = with; }
    public void ReactToTipGoalCreated(Func<JoystickTitleAmountEventArgs, Task> with) { OnJoystickTipGoalCreated = with; }
    public void ReactToTipGoalDeleted(Func<JoystickTitleAmountEventArgs, Task> with) { OnJoystickTipGoalDeleted = with; }
    public void ReactToTipGoalIncreased(Func<JoystickTipGoalIncreasedEventArgs, Task> with) { OnJoystickTipGoalIncreased = with; }
    public void ReactToTipGoalMet(Func<JoystickWhoWhatTitleAmountEventArgs, Task> with) { OnJoystickTipGoalMet = with; }
    public void ReactToTipGoalUpdated(Func<JoystickTitleAmountEventArgs, Task> with) { OnJoystickTipGoalUpdated = with; }
    public void ReactToTipMenuItemLocked(Func<JoystickTitleAmountEventArgs, Task> with) { OnJoystickTipMenuItemLocked = with; }
    public void ReactToTipMenuItemUnlocked(Func<JoystickTitleAmountEventArgs, Task> with) { OnJoystickTipMenuItemUnlocked = with; }
    public void ReactToChatTimerStarted(Func<JoystickChatTimerStartedEventArgs, Task> with) { OnJoystickChatTimerStarted = with; }
    public void ReactToChatTimersCleared(Func<JoystickWhoEventArgs, Task> with) { OnJoystickChatTimersCleared = with; }
    public void ReactToDropinStream(Func<JoystickDropinStreamEventArgs, Task> with) { OnJoystickDropinStream = with; }
    public void ReactToStreamDroppedIn(Func<JoystickStreamDroppedInEventArgs, Task> with) { OnJoystickStreamDroppedIn = with; }
    public void ReactToSubscribed(Func<JoystickWhoWhatAmountEventArgs, Task> with) { OnJoystickSubscribed = with; }
    public void ReactToResubscribed(Func<JoystickResubscribedEventArgs, Task> with) { OnJoystickResubscribed = with; }
    public void ReactToGiftedSubscriptions(Func<JoystickWhoWhatAmountEventArgs, Task> with) { OnJoystickGiftedSubscriptions = with; }
    public void ReactToWheelSpinClaimed(Func<JoystickWheelSpinClaimedEventArgs, Task> with) { OnJoystickWheelSpinClaimed = with; }
    public void ReactToViewerCountUpdated(Func<JoystickAmountEventArgs, Task> with) { OnJoystickViewerCountUpdated = with; }
    public void ReactToSubscriberCountUpdated(Func<JoystickAmountEventArgs, Task> with) { OnJoystickSubscriberCountUpdated = with; }
    public void ReactToMilestoneCompleted(Func<JoystickWhoWhatTitleAmountEventArgs, Task> with) { OnJoystickMilestoneCompleted = with; }
    public void ReactToPvpSessionRequested(Func<JoystickPvpSessionRequestedEventArgs, Task> with) { OnJoystickPvpSessionRequested = with; }
    public void ReactToPvpSessionReady(Func<JoystickPvpSessionReadyEventArgs, Task> with) { OnJoystickPvpSessionReady = with; }
    public void ReactToPvpSessionStarted(Func<JoystickPvpSessionStartedEventArgs, Task> with) { OnJoystickPvpSessionStarted = with; }
    public void ReactToPvpSessionEnding(Func<JoystickWhoWhatWhenWhereEventArgs, Task> with) { OnJoystickPvpSessionEnding = with; }
    public void ReactToPvpSessionEnded(Func<JoystickWhoWhatWhenWhereEventArgs, Task> with) { OnJoystickPvpSessionEnded = with; }
    public void ReactToSceneUpdated(Func<JoystickSceneUpdatedEventArgs, Task> with) { OnJoystickSceneUpdated = with; }
    public void ReactToSettingsUpdated(Func<JoystickEventArgs, Task> with) { OnJoystickSettingsUpdated = with; }
    public void ReactToStreamModeUpdated(Func<JoystickWhoEventArgs, Task> with) { OnJoystickStreamModeUpdated = with; }
    public void ReactToUserMuted(Func<JoystickWhoWhatEventArgs, Task> with) { OnJoystickUserMuted = with; }
    public void ReactToUserUnmuted(Func<JoystickWhoWhatEventArgs, Task> with) { OnJoystickUserUnmuted = with; }
    public void ReactToDeviceConnected(Func<JoystickEventArgs, Task> with) { OnJoystickDeviceConnected = with; }
    public void ReactToDeviceDisconnected(Func<JoystickEventArgs, Task> with) { OnJoystickDeviceDisconnected = with; }
    public void ReactToDeviceSettingsUpdated(Func<JoystickEventArgs, Task> with) { OnJoystickDeviceSettingsUpdated = with; }

    /* Actions */
    public void ChatMessage(string text) {
        if (_service == null || string.IsNullOrWhiteSpace(text)) return;
        _service.SendChatMessage(text);
    }

    public void DeleteMessage(string messageId) {
        if (_service == null || string.IsNullOrWhiteSpace(messageId)) return;
        _service.SendDeleteMessage(messageId);
    }

    public void Whisper(string username, string text) {
        if (_service == null || string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(text)) return;
        _service.SendWhisper(username, text);
    }

    public void MuteUser(string messageId) {
        if (_service == null || string.IsNullOrWhiteSpace(messageId)) return;
        _service.SendChatMessage(messageId);
    }

    public void UnmuteUser(string username) {
        if (_service == null || string.IsNullOrWhiteSpace(username)) return;
        _service.SendUnmuteUser(username);
    }

    public void BlockUser(string messageId) {
        if (_service == null || string.IsNullOrWhiteSpace(messageId)) return;
        _service.SendBlockUser(messageId);
    }
}
