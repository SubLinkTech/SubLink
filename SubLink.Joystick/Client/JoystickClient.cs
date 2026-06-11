using Serilog;
using SuperSocket.ClientEngine;
using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using WebSocket4Net;
using xyz.yewnyx.SubLink.Joystick.Client.Data.Command;
using xyz.yewnyx.SubLink.Joystick.Client.Data.Event;
using xyz.yewnyx.SubLink.Joystick.Client.Data.Message;
using xyz.yewnyx.SubLink.Joystick.Client.Data.Response;
using xyz.yewnyx.SubLink.Joystick.Client.OAuth;

namespace xyz.yewnyx.SubLink.Joystick.Client;

internal sealed class JoystickClient(ILogger logger) {
    private const string CUserAgent = "SubLink JoystickClient/1.0";
    internal static readonly JsonSerializerOptions _deserializationOpt = new() {
        AllowOutOfOrderMetadataProperties = true
    };
    internal static readonly JsonSerializerOptions _serializationOpt = new() {
        WriteIndented = false,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly ILogger _logger = logger;
    private WebSocket? _socket;
    private OAuthClient? _authClient;

    public event EventHandler? OnJoystickConnected;
    public event EventHandler? OnJoystickDisconnected;
    public event EventHandler<JoystickErrorEventArgs>? OnJoystickError;
    public event EventHandler<JoystickChatMessageEventArgs>? OnJoystickChatMessage;
    public event EventHandler<JoystickBotMessageEventArgs>? OnJoystickBotMessage;
    public event EventHandler<JoystickEnterStreamEventArgs>? OnJoystickEnterStream;
    public event EventHandler<JoystickLeaveStreamEventArgs>? OnJoystickLeaveStream;
    public event EventHandler<JoystickWhoEventArgs>? OnJoystickStarted;
    public event EventHandler<JoystickWhoEventArgs>? OnJoystickStreamResuming;
    public event EventHandler<JoystickWhoEventArgs>? OnJoystickStreamEnding;
    public event EventHandler<JoystickWhoEventArgs>? OnJoystickEnded;
    public event EventHandler<JoystickWhoWhatEventArgs>? OnJoystickFollowed;
    public event EventHandler<JoystickFollowerCountUpdatedEventArgs>? OnJoystickFollowerCountUpdated;
    public event EventHandler<JoystickTippedEventArgs>? OnJoystickTipped;
    public event EventHandler<JoystickTitleAmountEventArgs>? OnJoystickTipGoalCreated;
    public event EventHandler<JoystickTitleAmountEventArgs>? OnJoystickTipGoalDeleted;
    public event EventHandler<JoystickTipGoalIncreasedEventArgs>? OnJoystickTipGoalIncreased;
    public event EventHandler<JoystickWhoWhatTitleAmountEventArgs>? OnJoystickTipGoalMet;
    public event EventHandler<JoystickTitleAmountEventArgs>? OnJoystickTipGoalUpdated;
    public event EventHandler<JoystickTitleAmountEventArgs>? OnJoystickTipMenuItemLocked;
    public event EventHandler<JoystickTitleAmountEventArgs>? OnJoystickTipMenuItemUnlocked;
    public event EventHandler<JoystickChatTimerStartedEventArgs>? OnJoystickChatTimerStarted;
    public event EventHandler<JoystickWhoEventArgs>? OnJoystickChatTimersCleared;
    public event EventHandler<JoystickDropinStreamEventArgs>? OnJoystickDropinStream;
    public event EventHandler<JoystickStreamDroppedInEventArgs>? OnJoystickStreamDroppedIn;
    public event EventHandler<JoystickWhoWhatAmountEventArgs>? OnJoystickSubscribed;
    public event EventHandler<JoystickResubscribedEventArgs>? OnJoystickResubscribed;
    public event EventHandler<JoystickWhoWhatAmountEventArgs>? OnJoystickGiftedSubscriptions;
    public event EventHandler<JoystickWheelSpinClaimedEventArgs>? OnJoystickWheelSpinClaimed;
    public event EventHandler<JoystickAmountEventArgs>? OnJoystickViewerCountUpdated;
    public event EventHandler<JoystickAmountEventArgs>? OnJoystickSubscriberCountUpdated;
    public event EventHandler<JoystickWhoWhatTitleAmountEventArgs>? OnJoystickMilestoneCompleted;
    public event EventHandler<JoystickWhoWhatEventArgs>? OnJoystickUserMuted;
    public event EventHandler<JoystickWhoWhatEventArgs>? OnJoystickUserUnmuted;
    public event EventHandler<JoystickEventArgs>? OnJoystickDeviceConnected;
    public event EventHandler<JoystickEventArgs>? OnJoystickDeviceDisconnected;
    public event EventHandler<JoystickEventArgs>? OnJoystickDeviceSettingsUpdated;

    public bool Enabled { get; internal set; } = false;

    public async Task<bool> ConnectAsync(JoystickSettings settings) {
        if (_socket != null)
            return true;

        _authClient = new(_logger, settings.OAuthPort, settings.ClientId, settings.ClientSecret,
            settings.AccessToken, settings.RefreshToken, settings.State);

        try {
            // Do some oauth bullshit
            await _authClient.AuthorizeUser();

            if (!_authClient.IsAuthenticated) {
                _logger.Information("[{TAG}] visit https://joystick.tv/applications to create a new bot, then fill in ApplicationID, ClientID and ClientSecret in {CONFIGFILE}",
                    Platform.PlatformName, Platform.PlatformConfigFile);
                return false;
            }

            string json = await System.IO.File.ReadAllTextAsync(Platform.PlatformConfigFile);
            JsonNode? j = JsonNode.Parse(json, documentOptions: new() { CommentHandling = JsonCommentHandling.Skip });

#pragma warning disable CS8602 // Dereference of a possibly null reference.
            j[Platform.PlatformName]["AccessToken"] = _authClient.AccessToken;
            j[Platform.PlatformName]["RefreshToken"] = _authClient.RefreshToken;
            j[Platform.PlatformName]["State"] = _authClient.State;
            j[Platform.PlatformName]["Username"] = _authClient.Username;
            j[Platform.PlatformName]["ChannelId"] = _authClient.ChannelId;
            await System.IO.File.WriteAllTextAsync(Platform.PlatformConfigFile, j.ToJsonString(new() {
                WriteIndented = true,
                TypeInfoResolver = new DefaultJsonTypeInfoResolver()
            }));
#pragma warning restore CS8602 // Dereference of a possibly null reference.

            // Somehow Joystick now knows we are legit and can use the websocket API, magic
            _socket = new(
                $"wss://api.joystick.tv/cable?token={_authClient.AuthCode}",
                "actioncable-v1-json",
                version: WebSocketVersion.Rfc6455,
                userAgent: CUserAgent
            ) {
                EnableAutoSendPing = false,
                NoDelay = true
            };

            _socket.Opened += OnSockConnected;
            _socket.Closed += OnSockDisconnected;
            _socket.Error += OnSockError;
            _socket.MessageReceived += OnSockMessageReceived;
            _socket.DataReceived += OnSockDataReceived;

            await _socket.OpenAsync();
        } catch (Exception) {
            return false;
        }

        return true;
    }

    public async Task DisconnectAsync() {
        if (_socket == null) return;
        if (_socket.State != WebSocketState.Closed)
            await _socket.CloseAsync();

        _socket = null;
    }

    private void OnSockConnected(object? sender, EventArgs e) =>
        OnJoystickConnected?.Invoke(this, e);

    private void OnSockDisconnected(object? sender, EventArgs e) =>
        OnJoystickDisconnected?.Invoke(this, e);

    private void OnSockError(object? sender, ErrorEventArgs e) =>
        OnJoystickError?.Invoke(this, new(e.Exception));

    private void OnSockMessageReceived(object? sender, MessageReceivedEventArgs e) {
        var jsonObj = JsonDocument.Parse(e.Message, new JsonDocumentOptions { AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
        if (jsonObj == null) return;

        try {
            if (jsonObj.RootElement.TryGetProperty("type", out _)) {
                HandleResponseMessage(e.Message);
                return;
            }

            if (jsonObj.RootElement.TryGetProperty("identifier", out _)) {
                // Handle "message" events
                if (jsonObj.RootElement.TryGetProperty("message", out var msgObject) &&
                    msgObject.TryGetProperty("event", out var eventObject) &&
                    msgObject.TryGetProperty("type", out var typeObject)
                ) {
                    var messageJson = msgObject.GetRawText();

                    // Handle some specials that differ from the norm
                    if ("ChatMessage".Equals(eventObject.GetString(), StringComparison.OrdinalIgnoreCase) &&
                        "new_message".Equals(typeObject.GetString(), StringComparison.OrdinalIgnoreCase)
                    ) {
                        HandleChatMessage(messageJson);
                        return;
                    }

                    if ("BotMessage".Equals(eventObject.GetString(), StringComparison.OrdinalIgnoreCase) &&
                        "event_bot_message".Equals(typeObject.GetString(), StringComparison.OrdinalIgnoreCase)
                    ) {
                        HandleBotMessage(messageJson);
                        return;
                    }

                    // Handle "normal" events
                    HandleEventMessage(messageJson);
                    return;
                }

                // Place-holder I guess
            }

            _logger.Warning("[{TAG}] Unknown data received, message: {Message}", Platform.PlatformName, e.Message);
        } catch (Exception ex) {
            _logger.Error("[{TAG}] Exception raised while trying to handle JSON data:\nData: {Data}\nMessage: {Message}", Platform.PlatformName, e.Message, ex.ToString());
        }
    }

    private void OnSockDataReceived(object? sender, DataReceivedEventArgs e) =>
        _logger.Information("[{TAG}] Data received, length: {Length}", Platform.PlatformName, e.Data.Length);

    public void SendCommand(IBaseCommand cmd) {
        if (!Enabled) return;

        var jsonStr = JsonSerializer.Serialize(cmd, _serializationOpt);
        _logger.Information("[{TAG}] Sending: {jsonStr}", Platform.PlatformName, jsonStr);
        _socket?.Send(jsonStr);
    }

    public void SendString(string str) {
        if (!Enabled) return;

        _logger.Information("[{TAG}] Sending: {str}", Platform.PlatformName, str);
        _socket?.Send(str);
    }

    private void HandleResponseMessage(string message) {
        IBaseResponse? inMsg = JsonSerializer.Deserialize<IBaseResponse>(message, _deserializationOpt);
        if (inMsg == null) return;

        switch (inMsg) {
            case Welcome: {
                _logger.Information("[{TAG}] Welcome received", Platform.PlatformName);
                // We, SubLink, should only subscribe to the GatewayChannel
                SendCommand(new Subscribe());
                return;
            }
            case Ping: return; // Ignore, annoying and useless for anything other than keeping the socket alive
            case ConfirmSubscription: {
                ConfirmSubscription responseMsg = (ConfirmSubscription)inMsg;
                _logger.Information("[{TAG}] Confirmed subscription to event `{Channel}` for streamer `{StreamId}`",
                    Platform.PlatformName, responseMsg.Ident.Channel, responseMsg.Ident.StreamId);
                return;
            }
            case RejectSubscription: {
                RejectSubscription responseMsg = (RejectSubscription)inMsg;
                _logger.Information("[{TAG}] Rejected subscription to event `{Channel}` for streamer `{StreamId}`",
                    Platform.PlatformName, responseMsg.Ident.Channel, responseMsg.Ident.StreamId);
                return;
            }
            case ConfirmUnsubscription: {
                ConfirmUnsubscription responseMsg = (ConfirmUnsubscription)inMsg;
                _logger.Information("[{TAG}] Confirmed unsubscription to event `{Channel}` for streamer `{StreamId}`",
                    Platform.PlatformName, responseMsg.Ident.Channel, responseMsg.Ident.StreamId);
                return;
            }
            case RejectUnsubscription: {
                RejectUnsubscription responseMsg = (RejectUnsubscription)inMsg;
                _logger.Information("[{TAG}] Rejected unsubscription to event `{Channel}` for streamer `{StreamId}`",
                    Platform.PlatformName, responseMsg.Ident.Channel, responseMsg.Ident.StreamId);
                return;
            }
            default: {
                _logger.Warning("[{TAG}] Unknown data received, message: {Message}", Platform.PlatformName, message);
                return;
            }
        }
    }

    private void HandleChatMessage(string message) {
        var inMsg = JsonSerializer.Deserialize<NewMessageEvent>(message, _deserializationOpt);

        if (inMsg != null)
            OnJoystickChatMessage?.Invoke(this, new() {
                CreatedAt = inMsg.CreatedAt,
                Text = inMsg.Text,
                MessageId = inMsg.MessageId,
                Visibility = inMsg.Visibility,
                BotCommand = inMsg.BotCommand ?? string.Empty,
                BotCommandArg = inMsg.BotCommandArg ?? string.Empty,
                EmotesUsed = inMsg.EmotesUsed,
                AuthorSlug = inMsg.Author.Slug,
                AuthorUsername = inMsg.Author.Username,
                AuthorNickname = inMsg.Author.Nickname ?? string.Empty,
                AuthorIsStreamer = inMsg.Author.IsStreamer,
                AuthorIsModerator = inMsg.Author.IsModerator,
                AuthorIsSubscriber = inMsg.Author.IsSubscriber,
                AuthorIsVerified = inMsg.Author.IsVerified,
                AuthorIsContentCreator = inMsg.Author.IsContentCreator,
                StreamerSlug = inMsg.Streamer.Slug,
                StreamerUsername = inMsg.Streamer.Username,
                ChannelId = inMsg.ChannelId,
                Mention = inMsg.Mention,
                MentionedUsername = inMsg.MentionedUsername ?? string.Empty,
                Highlight = inMsg.Highlight
            });
    }

    private void HandleBotMessage(string message) {
        var inMsg = JsonSerializer.Deserialize<BotMessageEvent>(message, _deserializationOpt);

        if (inMsg != null)
            OnJoystickBotMessage?.Invoke(this, new()
            {
                CreatedAt = inMsg.CreatedAt,
                Text = inMsg.Text,
                MessageId = inMsg.MessageId,
                Visibility = inMsg.Visibility,
                EmotesUsed = inMsg.EmotesUsed,
                AuthorUsername = inMsg.Author.Username,
                Mention = inMsg.Mention
            });
    }

    private void HandleEventMessage(string message) {
        _logger.Warning("[{TAG}] HandleEventMessage, message: {Message}", Platform.PlatformName, message);
        IBaseEvent? inMsg = JsonSerializer.Deserialize<IBaseEvent>(message, _deserializationOpt);
        if (inMsg == null) return;

        switch (inMsg) {
            case EnterStreamEvent: {
                EnterStreamEvent eventMsg = (EnterStreamEvent)inMsg;
                OnJoystickEnterStream?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt));
                return;
            }
            case LeaveStreamEvent: {
                LeaveStreamEvent eventMsg = (LeaveStreamEvent)inMsg;
                OnJoystickLeaveStream?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt));
                return;
            }
            case StartedEvent: {
                StartedEvent eventMsg = (StartedEvent)inMsg;
                OnJoystickStarted?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who));
                return;
            }
            case StreamResumingEvent: {
                StreamResumingEvent eventMsg = (StreamResumingEvent)inMsg;
                OnJoystickStreamResuming?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who));
                return;
            }
            case StreamEndingEvent: {
                StreamEndingEvent eventMsg = (StreamEndingEvent)inMsg;
                OnJoystickStreamEnding?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who));
                return;
            }
            case EndedEvent: {
                EndedEvent eventMsg = (EndedEvent)inMsg;
                OnJoystickEnded?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who));
                return;
            }
            case FollowedEvent: {
                FollowedEvent eventMsg = (FollowedEvent)inMsg;
                OnJoystickFollowed?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who,
                    eventMsg.Metadata.What));
                return;
            }
            case FollowerCountUpdatedEvent: {
                FollowerCountUpdatedEvent eventMsg = (FollowerCountUpdatedEvent)inMsg;
                    OnJoystickFollowerCountUpdated?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt,
                        eventMsg.Metadata.NumberOfFollowers));
                return;
            }
            case TippedEvent: {
                TippedEvent eventMsg = (TippedEvent)inMsg;
                OnJoystickTipped?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who,
                    eventMsg.Metadata.What, eventMsg.Metadata.Amount, eventMsg.Metadata.TipMenuItem));
                return;
            }
            case TipGoalCreatedEvent: {
                TipGoalCreatedEvent eventMsg = (TipGoalCreatedEvent)inMsg;
                OnJoystickTipGoalCreated?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Title,
                    eventMsg.Metadata.Amount));
                return;
            }
            case TipGoalDeletedEvent: {
                TipGoalDeletedEvent eventMsg = (TipGoalDeletedEvent)inMsg;
                OnJoystickTipGoalDeleted?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Title,
                    eventMsg.Metadata.Amount));
                return;
            }
            case TipGoalIncreasedEvent: {
                TipGoalIncreasedEvent eventMsg = (TipGoalIncreasedEvent)inMsg;
                OnJoystickTipGoalIncreased?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.What,
                    eventMsg.Metadata.Amount, eventMsg.Metadata.ByUser, eventMsg.Metadata.Current, eventMsg.Metadata.Previous));
                return;
            }
            case TipGoalMetEvent: {
                TipGoalMetEvent eventMsg = (TipGoalMetEvent)inMsg;
                OnJoystickTipGoalMet?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who,
                    eventMsg.Metadata.What, eventMsg.Metadata.Title, eventMsg.Metadata.Amount));
                return;
            }
            case TipGoalUpdatedEvent: {
                TipGoalUpdatedEvent eventMsg = (TipGoalUpdatedEvent)inMsg;
                OnJoystickTipGoalUpdated?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Title,
                    eventMsg.Metadata.Amount));
                return;
            }
            case TipMenuItemLockedEvent: {
                TipMenuItemLockedEvent eventMsg = (TipMenuItemLockedEvent)inMsg;
                OnJoystickTipMenuItemLocked?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Title,
                    eventMsg.Metadata.Amount));
                return;
            }
            case TipMenuItemUnlockedEvent: {
                TipMenuItemUnlockedEvent eventMsg = (TipMenuItemUnlockedEvent)inMsg;
                OnJoystickTipMenuItemUnlocked?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Title,
                    eventMsg.Metadata.Amount));
                return;
            }
            case ChatTimerStartedEvent: {
                ChatTimerStartedEvent eventMsg = (ChatTimerStartedEvent)inMsg;
                OnJoystickChatTimerStarted?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Name,
                    eventMsg.Metadata.EndsAt));
                return;
            }
            case ChatTimersClearedEvent: {
                ChatTimersClearedEvent eventMsg = (ChatTimersClearedEvent)inMsg;
                OnJoystickChatTimersCleared?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who));
                return;
            }
            case DropinStreamEvent: {
                DropinStreamEvent eventMsg = (DropinStreamEvent)inMsg;
                OnJoystickDropinStream?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Origin,
                    eventMsg.Metadata.Destination, eventMsg.Metadata.NumberOfViewers, eventMsg.Metadata.DestinationUsername));
                return;
            }
            case StreamDroppedInEvent: {
                StreamDroppedInEvent eventMsg = (StreamDroppedInEvent)inMsg;
                OnJoystickStreamDroppedIn?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who,
                    eventMsg.Metadata.What, eventMsg.Metadata.NumberOfViewers));
                return;
            }
            case SubscribedEvent: {
                SubscribedEvent eventMsg = (SubscribedEvent)inMsg;
                OnJoystickSubscribed?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who,
                    eventMsg.Metadata.What, eventMsg.Metadata.HowMuch));
                return;
            }
            case ResubscribedEvent: {
                ResubscribedEvent eventMsg = (ResubscribedEvent)inMsg;
                OnJoystickResubscribed?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who,
                    eventMsg.Metadata.What, eventMsg.Metadata.HowMuch, eventMsg.Metadata.HowLong));
                return;
            }
            case GiftedSubscriptionsEvent: {
                GiftedSubscriptionsEvent eventMsg = (GiftedSubscriptionsEvent)inMsg;
                OnJoystickGiftedSubscriptions?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who,
                    eventMsg.Metadata.What, eventMsg.Metadata.HowMuch));
                return;
            }
            case WheelSpinClaimedEvent: {
                WheelSpinClaimedEvent eventMsg = (WheelSpinClaimedEvent)inMsg;
                OnJoystickWheelSpinClaimed?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who,
                    eventMsg.Metadata.What, eventMsg.Metadata.Amount, eventMsg.Metadata.Prize));
                return;
            }
            case ViewerCountUpdatedEvent: {
                ViewerCountUpdatedEvent eventMsg = (ViewerCountUpdatedEvent)inMsg;
                OnJoystickViewerCountUpdated?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt,
                    eventMsg.Metadata.NumberOfViewers));
                return;
            }
            case SubscriberCountUpdatedEvent: {
                SubscriberCountUpdatedEvent eventMsg = (SubscriberCountUpdatedEvent)inMsg;
                OnJoystickSubscriberCountUpdated?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt,
                    eventMsg.Metadata.NumberOfSubscribers));
                return;
            }
            case MilestoneCompletedEvent: {
                MilestoneCompletedEvent eventMsg = (MilestoneCompletedEvent)inMsg;
                OnJoystickMilestoneCompleted?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who,
                    eventMsg.Metadata.What, eventMsg.Metadata.Title, eventMsg.Metadata.Amount));
                return;
            }
            case UserMutedEvent: {
                UserMutedEvent eventMsg = (UserMutedEvent)inMsg;
                OnJoystickUserMuted?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who,
                    eventMsg.Metadata.What));
                return;
            }
            case UserUnmutedEvent: {
                UserUnmutedEvent eventMsg = (UserUnmutedEvent)inMsg;
                OnJoystickUserUnmuted?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt, eventMsg.Metadata.Who,
                    eventMsg.Metadata.What));
                return;
            }
            case DeviceConnectedEvent: {
                DeviceConnectedEvent eventMsg = (DeviceConnectedEvent)inMsg;
                OnJoystickDeviceConnected?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt));
                return;
            }
            case DeviceDisconnectedEvent: {
                DeviceDisconnectedEvent eventMsg = (DeviceDisconnectedEvent)inMsg;
                OnJoystickDeviceDisconnected?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt));
                return;
            }
            case DeviceSettingsUpdatedEvent: {
                DeviceSettingsUpdatedEvent eventMsg = (DeviceSettingsUpdatedEvent)inMsg;
                OnJoystickDeviceSettingsUpdated?.Invoke(this, new(eventMsg.Text, eventMsg.CreatedAt));
                return;
            }
            default: {
                _logger.Warning("[{TAG}] Unknown data received, message: {Message}", Platform.PlatformName, message);
                return;
            }
        }
    }
}
