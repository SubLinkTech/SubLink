using Newtonsoft.Json.Linq;
using Serilog;
using System;
using System.IO;
using System.IO.Pipes;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace xyz.yewnyx.SubLink.Discord.Client;

internal class DiscordClient(ILogger logger) : IDisposable {
    private NamedPipeClientStream? _pipeClient;
    private readonly object _writeLock = new();
    private readonly object _readLock = new();
    private readonly object _pipeLock = new();
    private CancellationTokenSource? _cts;
    private string _pipeName = string.Empty;
    private readonly ILogger _logger = logger;

    internal event EventHandler? OnNeedsRestart;
    public event EventHandler? OnReady;
    public event EventHandler<DiscordErrorArgs>? OnError;
    public event EventHandler<DiscordVoiceChannelIdEventArgs>? OnSelectedVoiceChannel;
    public event EventHandler<DiscordVoiceSettingsEventArgs>? OnVoiceSettingsUpdate;
    public event EventHandler<DiscordVoiceStatusEventArgs>? OnVoiceStatusUpdate;
    public event EventHandler<DiscordGuildIdEventArgs>? OnGuildStatus;
    public event EventHandler<DiscordGuildIdEventArgs>? OnGuildCreate;
    public event EventHandler<DiscordChannelEventArgs>? OnChannelCreate;
    public event EventHandler<DiscordUserIdEventArgs>? OnVoiceStateCreate;
    public event EventHandler<DiscordUserIdEventArgs>? OnVoiceStateUpdate;
    public event EventHandler<DiscordUserIdEventArgs>? OnVoiceStateDelete;
    public event EventHandler<DiscordUserIdEventArgs>? OnStartSpeaking;
    public event EventHandler<DiscordUserIdEventArgs>? OnStopSpeaking;
    public event EventHandler<DiscordMessageIdEventArgs>? OnMessageCreate;
    public event EventHandler<DiscordMessageIdEventArgs>? OnMessageUpdate;
    public event EventHandler<DiscordMessageIdEventArgs>? OnMessageDelete;
    public event EventHandler<DiscordChannelIdEventArgs>? OnNotificationCreate;
    public event EventHandler? OnActivityJoin;
    public event EventHandler? OnActivitySpectate;
    public event EventHandler<DiscordUserIdEventArgs>? OnActivityJoinRequest;

    public void Dispose() {
        StopListening();
        _pipeClient?.Dispose();
        _pipeClient = null;
    }

    private void InternalConnect() {
        if (_pipeClient != null) return;

        _pipeClient = new NamedPipeClientStream(
            serverName: ".",
            pipeName: _pipeName,
            direction: PipeDirection.InOut,
            options: PipeOptions.Asynchronous
        );
        _pipeClient.Connect(1000);
    }

    public async Task<bool> Connect(string pipeName) {
        _pipeName = pipeName;
        var connectTask = Task.Run(InternalConnect);
        var completedTask = await Task.WhenAny(
            connectTask,
            Task.Delay(TimeSpan.FromSeconds(2)) // 2-second timeout
        );

        if (completedTask == connectTask) { // Connection completed successfully
            try {
                await connectTask; // Ensure no exceptions occurred
                _logger.Information("[{TAG}] Successfully connected to {pipeName}", Platform.PlatformName, pipeName);
                return true;
            } catch (Exception ex) {
                _logger.Debug("[{TAG}] Error connecting to {pipeName}: {ERROR}", Platform.PlatformName, pipeName, ex.Message);
            }
        } else { // Timed out
            _logger.Debug("[{TAG}] Connection attempt to {pipeName} timed out.", Platform.PlatformName, pipeName);
        }

        return false;
    }

    private void Reconnect() {
        Dispose();
        InternalConnect();
        OnNeedsRestart?.Invoke(this, new());
    }

    public void StartListening() {
        if (_pipeClient == null || !_pipeClient.IsConnected)
            throw new InvalidOperationException("Must call Connect(...) before listening.");

        _cts = new CancellationTokenSource();
        Task.Run(() => {
            var header = new byte[8];

            try {
                while (!_cts.Token.IsCancellationRequested) {
                    lock (_readLock) {
                        int got = _pipeClient.Read(header, 0, 8);
                        if (got == 0) break; // pipe closed
                    }

                    int length = BitConverter.ToInt32(header, 4);
                    var body = new byte[length];

                    lock (_readLock) {
                        int read = 0;

                        while (read < length) {
                            read += _pipeClient.Read(body, read, length - read);
                        }
                    }

                    var jsonStr = Encoding.UTF8.GetString(body);
                    //_logger.Information("[{TAG}] JSON string received: {jsonStr}", Platform.PlatformName, jsonStr);
                    var evt = JsonSerializer.Deserialize<JsonElement>(jsonStr);
                    Task.Run(() => HandleRpcEvent(evt), _cts.Token);
                }
            } catch (IOException ex) {
                _logger.Error("[{TAG}] Error in pipe IO: {ERROR}", Platform.PlatformName, ex.Message);
            }
        }, _cts.Token);
    }

    public void StopListening() =>
        _cts?.Cancel();

    public string SendDataAndWait(int op, object payload) {
        if (_pipeClient == null || !_pipeClient.IsConnected)
            throw new InvalidOperationException("You must connect your client before sending events!");

        try {
            // Serialize payload to JSON
            string payloadJson = JsonSerializer.Serialize(payload);
            byte[] payloadBytes = Encoding.UTF8.GetBytes(payloadJson);

            // Create a header with operation code and payload length
            byte[] header = [
                .. BitConverter.GetBytes(op),
                .. BitConverter.GetBytes(payloadBytes.Length)
            ];

            string responseJson = string.Empty;
            lock (_pipeLock) {
                // Write header and payload to the pipe
                _pipeClient.Write(header, 0, header.Length);
                _pipeClient.Write(payloadBytes, 0, payloadBytes.Length);
                _pipeClient.Flush();
                _logger.Debug("[{TAG}] Payload sent: {PAYLOAD}", Platform.PlatformName, payloadJson);

                // Read the response
                byte[] responseHeader = new byte[8];
                _pipeClient.Read(responseHeader, 0, 8);
                int statusCode = BitConverter.ToInt32(responseHeader, 0);
                int responseLength = BitConverter.ToInt32(responseHeader, 4);

                byte[] responseBytes = new byte[responseLength];
                _pipeClient.Read(responseBytes, 0, responseLength);
                responseJson = Encoding.UTF8.GetString(responseBytes);
            }
            _logger.Debug("[{TAG}] Response received: {RESPONSE}", Platform.PlatformName, responseJson);
            return responseJson;
        } catch (IOException ex) {
            _logger.Error("[{TAG}] Pipe communication error: {ERROR}", Platform.PlatformName, ex.Message);

            if (ex.Message.Contains("broken")) {
                _logger.Debug("[{TAG}] Attempting to reconnect...", Platform.PlatformName);
                Reconnect();
                return SendDataAndWait(op, payload); // Retry sending the payload
            }

            return string.Empty;
        }
    }

    public void SendCommand(int op, object payload) {
        if (_pipeClient == null || !_pipeClient.IsConnected)
            throw new InvalidOperationException("Must call Connect(...) before sending commands.");

        // 1) Serialize payload
        string json = JsonSerializer.Serialize(payload);
        byte[] body = Encoding.UTF8.GetBytes(json);

        // 2) Build 8-byte header: [ op (4 bytes) | length (4 bytes) ]
        byte[] header = [
            .. BitConverter.GetBytes(op),
            .. BitConverter.GetBytes(body.Length)
        ];

        // 3) Atomically write header + body
        lock (_writeLock) {
            _pipeClient.Write(header, 0, header.Length);
            _pipeClient.Write(body, 0, body.Length);
            _pipeClient.Flush();
        }
    }

    public async Task<string> Handshake(string clientId) {
        // Timeout for the handshake process
        var handshakeTask = Task.Run(() => SendDataAndWait(0, DiscordIpcMessage.Handshake(clientId)));
        var completedHandshake = await Task.WhenAny(
            handshakeTask,
            Task.Delay(TimeSpan.FromSeconds(3)) // 3-second timeout for handshake
        );

        if (completedHandshake == handshakeTask) {
            _logger.Debug("[{TAG}] Handshake completed successfully.", Platform.PlatformName);
            return await handshakeTask;
        } else {
            _logger.Warning("[{TAG}] Handshake timed out. Try restarting discord.", Platform.PlatformName);
            return string.Empty;
        }
    }

    public async Task<bool> Authenticate(string token) {
        try {
            // Timeout for the authentication process
            var authTask = Task.Run(() => SendDataAndWait(1, DiscordIpcMessage.Authenticate(token)));
            var completedAuth = await Task.WhenAny(
                authTask,
                Task.Delay(TimeSpan.FromSeconds(3)) // 3-second timeout for authentication
            );

            if (completedAuth == authTask) {
                var authResponse = await authTask;
                return true;
            }
        } catch (Exception ex) {
            _logger.Error("[{TAG}] Authentication failure: {ERROR}", Platform.PlatformName, ex.Message);
        }

        return false;
    }

    private static int VoiceStateToInt(string state) =>
        state switch {
            "DISCONNECTED" => 0,
            "AWAITING_ENDPOINT" => 1,
            "AUTHENTICATING" => 2,
            "CONNECTING" => 3,
            "CONNECTED" => 4,
            "VOICE_DISCONNECTED" => 5,
            "VOICE_CONNECTING" => 6,
            "VOICE_CONNECTED" => 7,
            "NO_ROUTE" => 8,
            "ICE_CHECKING" => 9,
            _ => -1
        };

    private static int EventNameToInt(string evtName) =>
        evtName switch {
            "READY" => 0,
            "ERROR" => 1,
            "GUILD_STATUS" => 2,
            "GUILD_CREATE" => 3,
            "CHANNEL_CREATE" => 4,
            "VOICE_CHANNEL_SELECT" => 5,
            "VOICE_STATE_CREATE" => 6,
            "VOICE_STATE_UPDATE" => 7,
            "VOICE_STATE_DELETE" => 8,
            "VOICE_SETTINGS_UPDATE" => 9,
            "VOICE_CONNECTION_STATUS" => 10,
            "SPEAKING_START" => 11,
            "SPEAKING_STOP" => 12,
            "MESSAGE_CREATE" => 13,
            "MESSAGE_UPDATE" => 14,
            "MESSAGE_DELETE" => 15,
            "NOTIFICATION_CREATE" => 16,
            "ACTIVITY_JOIN" => 17,
            "ACTIVITY_SPECTATE" => 18,
            "ACTIVITY_JOIN_REQUEST" => 19,
            _ => -1
        };

    private void HandleRpcEvent(JsonElement msg) {
        try {
            _logger.Debug("[{TAG}] Message received: {msg}", Platform.PlatformName, msg);

            if (!msg.TryGetProperty("cmd", out var commandNameEl)) {
                _logger.Error($"Unsupported message: {JsonSerializer.Serialize(msg)}");
                return;
            }

            string commandName = commandNameEl.GetString() ?? string.Empty;

            if (commandName.Equals("SUBSCRIBE", StringComparison.InvariantCultureIgnoreCase)) return;

            if (!msg.TryGetProperty("evt", out var eventNameEl)) {
                _logger.Error($"Unsupported message: {JsonSerializer.Serialize(msg)}");
                return;
            }

            string eventName = eventNameEl.GetString() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(eventName))
                HandleCommandResponse(commandName, msg);
            else
                HandleEvent(eventName, msg);
        } catch (Exception ex) {
            _logger.Warning("[{TAG}] Failed to handle RPC event: {ERROR}", Platform.PlatformName, ex.Message);
        }
    }

    private void HandleCommandResponse(string commandName, JsonElement resp) {
        switch (commandName) {
            case "GET_VOICE_SETTINGS": {
                if (resp.TryGetProperty("data", out var vs))
                    OnVoiceSettingsUpdate?.Invoke(this, new(
                        vs.GetProperty("input").GetProperty("volume").GetSingle(),
                        vs.GetProperty("output").GetProperty("volume").GetSingle(),
                        vs.GetProperty("mode").GetProperty("type").GetString() ?? string.Empty,
                        vs.GetProperty("mode").GetProperty("auto_threshold").GetBoolean(),
                        vs.GetProperty("mode").GetProperty("threshold").GetSingle(),
                        vs.GetProperty("mode").GetProperty("delay").GetSingle(),
                        vs.GetProperty("automatic_gain_control").GetBoolean(),
                        vs.GetProperty("echo_cancellation").GetBoolean(),
                        vs.GetProperty("noise_suppression").GetBoolean(),
                        vs.GetProperty("qos").GetBoolean(),
                        vs.GetProperty("silence_warning").GetBoolean(),
                        vs.GetProperty("deaf").GetBoolean(),
                        vs.GetProperty("mute").GetBoolean()
                    ));

                return;
            }
            case "GET_GUILD": {
                if (resp.TryGetProperty("data", out var gs) && gs.TryGetProperty("guild", out var g) && g.TryGetProperty("id", out var gidElStatus))
                    OnGuildStatus?.Invoke(this, new(gidElStatus.GetString() ?? string.Empty));

                return;
            }
            case "GET_CHANNEL":
            case "GET_SELECTED_VOICE_CHANNEL": {
                if (resp.TryGetProperty("data", out var vc) &&
                    vc.TryGetProperty("id", out var cid) &&
                    vc.TryGetProperty("guild_id", out var gid))
                    OnSelectedVoiceChannel?.Invoke(this, new(cid.GetString() ?? string.Empty, gid.GetString() ?? string.Empty));

                return;
            }
            default: return; // Ignore
        }
    }

    private void HandleEvent(string eventName, JsonElement evt) {
        int evtCode = EventNameToInt(eventName);

        switch (eventName) {
            case "READY": {
                OnReady?.Invoke(this, new());
                return;
            }
            case "ERROR": {
                if (evt.TryGetProperty("data", out var err) && err.TryGetProperty("code", out var codeEl) && err.TryGetProperty("message", out var msgEl))
                    OnError?.Invoke(this, new(codeEl.GetInt32(), msgEl.GetString() ?? string.Empty));

                return;
            }
            case "VOICE_CHANNEL_SELECT": {
                if (evt.TryGetProperty("data", out var vc) &&
                    vc.TryGetProperty("channel_id", out var cid) &&
                    vc.TryGetProperty("guild_id", out var gid))
                    OnSelectedVoiceChannel?.Invoke(this, new(cid.GetString() ?? string.Empty, gid.GetString() ?? string.Empty));

                return;
            }
            case "VOICE_SETTINGS_UPDATE": {
                if (evt.TryGetProperty("data", out var vs))
                    OnVoiceSettingsUpdate?.Invoke(this, new(
                        vs.GetProperty("input").GetProperty("volume").GetSingle(),
                        vs.GetProperty("output").GetProperty("volume").GetSingle(),
                        vs.GetProperty("mode").GetProperty("type").GetString() ?? string.Empty,
                        vs.GetProperty("mode").GetProperty("auto_threshold").GetBoolean(),
                        vs.GetProperty("mode").GetProperty("threshold").GetSingle(),
                        vs.GetProperty("mode").GetProperty("delay").GetSingle(),
                        vs.GetProperty("automatic_gain_control").GetBoolean(),
                        vs.GetProperty("echo_cancellation").GetBoolean(),
                        vs.GetProperty("noise_suppression").GetBoolean(),
                        vs.GetProperty("qos").GetBoolean(),
                        vs.GetProperty("silence_warning").GetBoolean(),
                        vs.GetProperty("deaf").GetBoolean(),
                        vs.GetProperty("mute").GetBoolean()
                    ));

                return;
            }
            case "VOICE_CONNECTION_STATUS": {
                if (evt.TryGetProperty("data", out var vcstat) && vcstat.TryGetProperty("state", out var stateEl)) {
                    string state = stateEl.GetString() ?? string.Empty;
                    OnVoiceStatusUpdate?.Invoke(this, new(state, VoiceStateToInt(state)));
                }

                return;
            }
            case "GUILD_STATUS": {
                if (evt.TryGetProperty("data", out var gs) && gs.TryGetProperty("guild", out var g) && g.TryGetProperty("id", out var gidElStatus))
                    OnGuildStatus?.Invoke(this, new(gidElStatus.GetString() ?? string.Empty));

                return;
            }
            case "GUILD_CREATE": {
                if (evt.TryGetProperty("data", out var gc) && gc.TryGetProperty("id", out var gidc))
                    OnGuildCreate?.Invoke(this, new(gidc.GetString() ?? string.Empty));

                return;
            }
            case "CHANNEL_CREATE": {
                if (evt.TryGetProperty("data", out var ch) && ch.TryGetProperty("id", out var cidEl) && ch.TryGetProperty("name", out var cnameEl))
                    OnChannelCreate?.Invoke(this, new(
                        cidEl.GetString() ?? string.Empty,
                        cnameEl.GetString() ?? string.Empty
                    ));

                return;
            }
            case "VOICE_STATE_CREATE": {
                if (evt.TryGetProperty("data", out var vsd) && vsd.TryGetProperty("user", out var usr) && usr.TryGetProperty("id", out var uid))
                    OnVoiceStateCreate?.Invoke(this, new(uid.GetString() ?? string.Empty));

                return;
            }
            case "VOICE_STATE_UPDATE": {
                if (evt.TryGetProperty("data", out var vsd) && vsd.TryGetProperty("user", out var usr) && usr.TryGetProperty("id", out var uid))
                    OnVoiceStateUpdate?.Invoke(this, new(uid.GetString() ?? string.Empty));

                return;
            }
            case "VOICE_STATE_DELETE": {
                if (evt.TryGetProperty("data", out var vsd) && vsd.TryGetProperty("user", out var usr) && usr.TryGetProperty("id", out var uid))
                    OnVoiceStateDelete?.Invoke(this, new(uid.GetString() ?? string.Empty));

                return;
            }
            case "SPEAKING_START": {
                if (evt.TryGetProperty("data", out var sp) && sp.TryGetProperty("user_id", out var suid))
                    OnStartSpeaking?.Invoke(this, new(suid.GetString() ?? string.Empty));

                return;
            }
            case "SPEAKING_STOP": {
                if (evt.TryGetProperty("data", out var sp) && sp.TryGetProperty("user_id", out var suid))
                    OnStopSpeaking?.Invoke(this, new(suid.GetString() ?? string.Empty));

                return;
            }
            case "MESSAGE_CREATE": {
                if (evt.TryGetProperty("data", out var msg) && msg.TryGetProperty("message", out var m) && m.TryGetProperty("id", out var mid))
                    OnMessageCreate?.Invoke(this, new(mid.GetString() ?? string.Empty));

                return;
            }
            case "MESSAGE_UPDATE": {
                if (evt.TryGetProperty("data", out var msg) && msg.TryGetProperty("message", out var m) && m.TryGetProperty("id", out var mid))
                    OnMessageUpdate?.Invoke(this, new(mid.GetString() ?? string.Empty));

                return;
            }
            case "MESSAGE_DELETE": {
                if (evt.TryGetProperty("data", out var msg) && msg.TryGetProperty("message", out var m) && m.TryGetProperty("id", out var mid))
                    OnMessageDelete?.Invoke(this, new(mid.GetString() ?? string.Empty));

                return;
            }
            case "NOTIFICATION_CREATE": {
                if (evt.TryGetProperty("data", out var noti) && noti.TryGetProperty("channel_id", out var nch))
                    OnNotificationCreate?.Invoke(this, new(nch.GetString() ?? string.Empty));

                return;
            }
            case "ACTIVITY_JOIN": {
                OnActivityJoin?.Invoke(this, new());
                return;
            }
            case "ACTIVITY_SPECTATE": {
                OnActivitySpectate?.Invoke(this, new());
                return;
            }
            case "ACTIVITY_JOIN_REQUEST": {
                if (evt.TryGetProperty("data", out var aj) && aj.TryGetProperty("user", out var aju) && aju.TryGetProperty("id", out var ajuId))
                    OnActivityJoinRequest?.Invoke(this, new(ajuId.GetString() ?? string.Empty));

                return;
            }
            default: {
                _logger.Error($"Unsupported event: {JsonSerializer.Serialize(evt)}");
                return;
            }
        }
    }
}
