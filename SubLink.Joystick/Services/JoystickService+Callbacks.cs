using System;
using System.Threading.Tasks;
using xyz.yewnyx.SubLink.Joystick.Client;

namespace xyz.yewnyx.SubLink.Joystick.Services;

internal sealed partial class JoystickService {
    private void WireCallbacks() {
        _client.OnJoystickConnected += OnJoystickConnected;
        _client.OnJoystickDisconnected += OnJoystickDisconnected;
        _client.OnJoystickError += OnJoystickError;
        _client.OnJoystickChatMessage += OnJoystickChatMessage;
        _client.OnJoystickBotMessage += OnJoystickBotMessage;
        _client.OnJoystickEnterStream += OnJoystickEnterStream;
        _client.OnJoystickLeaveStream += OnJoystickLeaveStream;
        _client.OnJoystickStarted += OnJoystickStarted;
        _client.OnJoystickStreamResuming += OnJoystickStreamResuming;
        _client.OnJoystickStreamEnding += OnJoystickStreamEnding;
        _client.OnJoystickEnded += OnJoystickEnded;
        _client.OnJoystickFollowed += OnJoystickFollowed;
        _client.OnJoystickFollowerCountUpdated += OnJoystickFollowerCountUpdated;
        _client.OnJoystickTipped += OnJoystickTipped;
        _client.OnJoystickTipGoalCreated += OnJoystickTipGoalCreated;
        _client.OnJoystickTipGoalDeleted += OnJoystickTipGoalDeleted;
        _client.OnJoystickTipGoalIncreased += OnJoystickTipGoalIncreased;
        _client.OnJoystickTipGoalMet += OnJoystickTipGoalMet;
        _client.OnJoystickTipGoalUpdated += OnJoystickTipGoalUpdated;
        _client.OnJoystickTipMenuItemLocked += OnJoystickTipMenuItemLocked;
        _client.OnJoystickTipMenuItemUnlocked += OnJoystickTipMenuItemUnlocked;
        _client.OnJoystickChatTimerStarted += OnJoystickChatTimerStarted;
        _client.OnJoystickChatTimersCleared += OnJoystickChatTimersCleared;
        _client.OnJoystickDropinStream += OnJoystickDropinStream;
        _client.OnJoystickStreamDroppedIn += OnJoystickStreamDroppedIn;
        _client.OnJoystickSubscribed += OnJoystickSubscribed;
        _client.OnJoystickResubscribed += OnJoystickResubscribed;
        _client.OnJoystickGiftedSubscriptions += OnJoystickGiftedSubscriptions;
        _client.OnJoystickWheelSpinClaimed += OnJoystickWheelSpinClaimed;
        _client.OnJoystickViewerCountUpdated += OnJoystickViewerCountUpdated;
        _client.OnJoystickSubscriberCountUpdated += OnJoystickSubscriberCountUpdated;
        _client.OnJoystickMilestoneCompleted += OnJoystickMilestoneCompleted;
        _client.OnJoystickDeviceConnected += OnJoystickDeviceConnected;
        _client.OnJoystickDeviceDisconnected += OnJoystickDeviceDisconnected;
        _client.OnJoystickDeviceSettingsUpdated += OnJoystickDeviceSettingsUpdated;
    }

    private void OnJoystickConnected(object? sender, EventArgs e) =>
        _logger.Information("[{TAG}] Connected to websocket", Platform.PlatformName);

    private void OnJoystickDisconnected(object? sender, EventArgs e) =>
        _logger.Warning("[{TAG}] Disconnected from websocket", Platform.PlatformName);

    private void OnJoystickError(object? sender, JoystickErrorEventArgs e) =>
        _logger.Error("[{TAG}] Error ocured with the websocket\r\n{Exception}", Platform.PlatformName, e.Exception);

    private void OnJoystickChatMessage(object? sender, JoystickChatMessageEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickChatMessage: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickBotMessage(object? sender, JoystickBotMessageEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickBotMessage: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickEnterStream(object? sender, JoystickEnterStreamEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickEnterStream: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickLeaveStream(object? sender, JoystickLeaveStreamEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickLeaveStream: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickStarted(object? sender, JoystickWhoEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickStarted: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickStreamResuming(object? sender, JoystickWhoEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickStreamResuming: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickStreamEnding(object? sender, JoystickWhoEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickStreamEnding: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickEnded(object? sender, JoystickWhoEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickEnded: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickFollowed(object? sender, JoystickWhoWhatEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickFollowed: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickFollowerCountUpdated(object? sender, JoystickFollowerCountUpdatedEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickFollowerCountUpdated: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickTipped(object? sender, JoystickTippedEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickTipped: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickTipGoalCreated(object? sender, JoystickTitleAmountEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickTipGoalCreated: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickTipGoalDeleted(object? sender, JoystickTitleAmountEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickTipGoalDeleted: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickTipGoalIncreased(object? sender, JoystickTipGoalIncreasedEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickTipGoalIncreased: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickTipGoalMet(object? sender, JoystickWhoWhatTitleAmountEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickTipGoalMet: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickTipGoalUpdated(object? sender, JoystickTitleAmountEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickTipGoalUpdated: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickTipMenuItemLocked(object? sender, JoystickTitleAmountEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickTipMenuItemLocked: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickTipMenuItemUnlocked(object? sender, JoystickTitleAmountEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickTipMenuItemUnlocked: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickChatTimerStarted(object? sender, JoystickChatTimerStartedEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickChatTimerStarted: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickChatTimersCleared(object? sender, JoystickWhoEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickChatTimersCleared: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickDropinStream(object? sender, JoystickDropinStreamEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickDropinStream: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickStreamDroppedIn(object? sender, JoystickStreamDroppedInEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickStreamDroppedIn: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickSubscribed(object? sender, JoystickWhoWhatAmountEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickSubscribed: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickResubscribed(object? sender, JoystickResubscribedEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickResubscribed: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickGiftedSubscriptions(object? sender, JoystickWhoWhatAmountEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickGiftedSubscriptions: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickWheelSpinClaimed(object? sender, JoystickWheelSpinClaimedEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickWheelSpinClaimed: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickViewerCountUpdated(object? sender, JoystickAmountEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickViewerCountUpdated: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickSubscriberCountUpdated(object? sender, JoystickAmountEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickSubscriberCountUpdated: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickMilestoneCompleted(object? sender, JoystickWhoWhatTitleAmountEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickMilestoneCompleted: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickUserMuted(object? sender, JoystickWhoWhatEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickUserMuted: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickUserUnmuted(object? sender, JoystickWhoWhatEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickUserUnmuted: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickDeviceConnected(object? sender, JoystickEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickDeviceConnected: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickDeviceDisconnected(object? sender, JoystickEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickDeviceDisconnected: { } callback })
                await callback(e);
        });
    }

    private void OnJoystickDeviceSettingsUpdated(object? sender, JoystickEventArgs e) {
        Task.Run(async () => {
            if (_rules is JoystickRules { OnJoystickDeviceSettingsUpdated: { } callback })
                await callback(e);
        });
    }
}
