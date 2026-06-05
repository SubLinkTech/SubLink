using Serilog;
using SuperSocket.ClientEngine;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using WebSocket4Net;
using xyz.yewnyx.SubLink.Joystick.Client.Data;
using xyz.yewnyx.SubLink.Joystick.Client.Data.Command;
using xyz.yewnyx.SubLink.Joystick.Client.Data.Event;
using xyz.yewnyx.SubLink.Joystick.Client.Data.Response;

namespace xyz.yewnyx.SubLink.Joystick.Client;

internal sealed class JoystickClient(ILogger logger) {
    private const string _userAgent = "SubLink JoystickClient/1.0";
    private static readonly JsonSerializerOptions _serializationOpt = new() {
        AllowOutOfOrderMetadataProperties = true
    };

    private readonly ILogger _logger = logger;
    private WebSocket? _socket;
    private string _authCode = string.Empty;

    public event EventHandler? OnJoystickConnected;
    public event EventHandler? OnJoystickDisconnected;
    public event EventHandler<JoystickErrorEventArgs>? OnJoystickError;

    public bool Enabled { get; internal set; } = false;

    public async Task<bool> ConnectAsync(JoystickSettings settings) {
        if (_socket != null) return true;

        _authCode = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{settings.ClientId}:{settings.ClientSecret}"));

        try {
            // Do some oauth bullshit

            // Somehow Joystick now knows we are legit and can use the websocket API, magic
            _socket = new(
                $"wss://api.joystick.tv/cable?token={_authCode}",
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

    public void SendData(BaseCommand cmd) {
        if (!Enabled) return;

        _socket?.Send(JsonSerializer.Serialize(cmd));
    }

    private void HandleResponseMessage(string message) {
        IBaseResponse? inMsg = JsonSerializer.Deserialize<IBaseResponse>(message, _serializationOpt);
        if (inMsg == null) return;

        switch (inMsg) {
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
            default: {
                _logger.Warning("[{TAG}] Unknown data received, message: {Message}", Platform.PlatformName, message);
                return;
            }
        }
    }
}
