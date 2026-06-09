using Serilog;
using SuperSocket.ClientEngine;
using System;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using WebSocket4Net;
using xyz.yewnyx.SubLink.Joystick.Client.Data;
using xyz.yewnyx.SubLink.Joystick.Client.Data.Command;
using xyz.yewnyx.SubLink.Joystick.Client.Data.Event;
using xyz.yewnyx.SubLink.Joystick.Client.Data.Response;
using xyz.yewnyx.SubLink.Joystick.Client.OAuth;

namespace xyz.yewnyx.SubLink.Joystick.Client;

internal sealed class JoystickClient(ILogger logger) {
    private const string _userAgent = "SubLink JoystickClient/1.0";
    private static readonly JsonSerializerOptions _serializationOpt = new() {
        AllowOutOfOrderMetadataProperties = true
    };

    private readonly ILogger _logger = logger;
    private WebSocket? _socket;
    private OAuthClient? _authClient;

    public event EventHandler? OnJoystickConnected;
    public event EventHandler? OnJoystickDisconnected;
    public event EventHandler<JoystickErrorEventArgs>? OnJoystickError;

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
                userAgent: _userAgent
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

            // We, SubLink, should only subscribe to the GatewayChannel
            SendData(new Subscribe());
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
        var jsonObj = JsonDocument.Parse(e.Message, new JsonDocumentOptions { MaxDepth = 1, AllowTrailingCommas = true, CommentHandling = JsonCommentHandling.Skip });
        if (jsonObj == null) return;

        if (jsonObj.RootElement.TryGetProperty("type", out _)) {
            HandleResponseMessage(e.Message);
            return;
        }

        if (jsonObj.RootElement.TryGetProperty("identifier", out _)) {
            HandleEventMessage(e.Message);
            return;
        }

        _logger.Warning("[{TAG}] Unknown data received, message: {Message}", Platform.PlatformName, e.Message);
    }

    private void OnSockDataReceived(object? sender, DataReceivedEventArgs e) =>
        _logger.Information("[{TAG}] Data received, length: {Length}", Platform.PlatformName, e.Data.Length);

    public void SendData(IBaseCommand cmd) {
        if (!Enabled) return;

        _socket?.Send(JsonSerializer.Serialize(cmd));
    }

    private void HandleResponseMessage(string message) {
        IBaseResponse? inMsg = JsonSerializer.Deserialize<IBaseResponse>(message, _serializationOpt);
        if (inMsg == null) return;

        switch (inMsg) {
            case Welcome: {
                _logger.Information("[{TAG}] Welcome received", Platform.PlatformName);
                return;
            }
            case Ping: {
                Ping responseMsg = (Ping)inMsg;
                _logger.Information("[{TAG}] Ping received {Timestamp}", Platform.PlatformName, responseMsg.Message);
                return;
            }
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

    private void HandleEventMessage(string message) {
        IBaseEvent? inMsg = JsonSerializer.Deserialize<IBaseEvent>(message, _serializationOpt);
        if (inMsg == null) return;

        switch (inMsg) {
            case StartedEvent: {
                StartedEvent eventMsg = (StartedEvent)inMsg;
                return;
            }
            default: {
                _logger.Warning("[{TAG}] Unknown data received, message: {Message}", Platform.PlatformName, message);
                return;
            }
        }
    }
}
