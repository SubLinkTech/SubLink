using Serilog;
using System;
using System.IO;
using System.IO.Pipes;
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
    public event EventHandler<EventArgs>? OnReady;
    public event EventHandler<DiscordErrorArgs>? OnError;
    public event EventHandler<DiscordGuildInfoEventArgs>? OnGuildStatus;
    public event EventHandler<DiscordGuildInfoEventArgs>? OnGuildCreate;
    public event EventHandler<DiscordChannelCreateEventArgs>? OnChannelCreate;
    public event EventHandler<DiscordVoiceChannelIdEventArgs>? OnSelectedVoiceChannel;
    public event EventHandler<DiscordVoiceSettingsEventArgs>? OnVoiceSettingsUpdate;
    public event EventHandler<DiscordVoiceStateEventArgs>? OnVoiceStateCreate;
    public event EventHandler<DiscordVoiceStateEventArgs>? OnVoiceStateUpdate;
    public event EventHandler<DiscordVoiceStateEventArgs>? OnVoiceStateDelete;
    public event EventHandler<DiscordVoiceStatusEventArgs>? OnVoiceStatusUpdate;
    public event EventHandler<DiscordMessageEventArgs>? OnMessageCreate;
    public event EventHandler<DiscordMessageEventArgs>? OnMessageUpdate;
    public event EventHandler<DiscordMessageEventArgs>? OnMessageDelete;
    public event EventHandler<DiscordUserIdEventArgs>? OnStartSpeaking;
    public event EventHandler<DiscordUserIdEventArgs>? OnStopSpeaking;
    public event EventHandler<DiscordNotificationEventArgs>? OnNotificationCreate;
    public event EventHandler<EventArgs>? OnActivityJoin;
    public event EventHandler<EventArgs>? OnActivitySpectate;
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
        string payloadJson = JsonSerializer.Serialize(payload);
        byte[] payloadBytes = Encoding.UTF8.GetBytes(payloadJson);

        // 2) Build 8-byte header: [ op (4 bytes) | length (4 bytes) ]
        byte[] header = [
            .. BitConverter.GetBytes(op),
            .. BitConverter.GetBytes(payloadBytes.Length)
        ];

        // 3) Atomically write header + body
        lock (_writeLock) {
            _pipeClient.Write(header, 0, header.Length);
            _pipeClient.Write(payloadBytes, 0, payloadBytes.Length);
            _pipeClient.Flush();
            _logger.Debug("[{TAG}] Payload sent: {PAYLOAD}", Platform.PlatformName, payloadJson);
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

    public void FireOnReadyEvent() =>
        OnReady?.Invoke(this, new());

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
                if (resp.TryGetProperty("data", out var vs) &&
                    vs.TryGetProperty("mode", out var vsMode))
                    OnVoiceSettingsUpdate?.Invoke(this, new(
                        vs.GetProperty("input").GetProperty("volume").GetSingle(),
                        vs.GetProperty("output").GetProperty("volume").GetSingle(),
                        vsMode.GetProperty("type").GetString() ?? string.Empty,
                        vsMode.GetProperty("auto_threshold").GetBoolean(),
                        vsMode.GetProperty("threshold").GetSingle(),
                        vsMode.GetProperty("delay").GetSingle(),
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
                if (resp.TryGetProperty("data", out var gs))
                    OnGuildStatus?.Invoke(this, new(
                        gs.GetProperty("id").GetString() ?? string.Empty,
                        gs.GetProperty("name").GetString() ?? string.Empty,
                        gs.GetProperty("icon_url").GetString() ?? string.Empty
                    ));

                return;
            }
            case "GET_CHANNEL":
            case "GET_SELECTED_VOICE_CHANNEL": {
                if (resp.TryGetProperty("data", out var vc))
                    OnSelectedVoiceChannel?.Invoke(this, new(
                        vc.GetProperty("id").GetString() ?? string.Empty,
                        vc.GetProperty("guild_id").GetString() ?? string.Empty
                    ));

                return;
            }
            default: return; // Ignore
        }
    }

    private void HandleEvent(string eventName, JsonElement evt) {
        int evtCode = EventNameToInt(eventName);

        switch (eventName) {
            case "ERROR": {
                if (evt.TryGetProperty("data", out var err))
                    OnError?.Invoke(this, new(
                        err.GetProperty("code").GetInt32(),
                        err.GetProperty("message").GetString() ?? string.Empty
                    ));

                return;
            }
            case "GUILD_STATUS": {
                if (evt.TryGetProperty("data", out var gs) && gs.TryGetProperty("guild", out var g))
                    OnGuildStatus?.Invoke(this, new(
                        g.GetProperty("id").GetString() ?? string.Empty,
                        g.GetProperty("name").GetString() ?? string.Empty,
                        g.GetProperty("icon_url").GetString() ?? string.Empty
                    ));

                return;
            }
            case "GUILD_CREATE": {
                if (evt.TryGetProperty("data", out var gc))
                    OnGuildCreate?.Invoke(this, new(
                        gc.GetProperty("id").GetString() ?? string.Empty,
                        gc.GetProperty("name").GetString() ?? string.Empty,
                        string.Empty
                    ));

                return;
            }
            case "CHANNEL_CREATE": {
                if (evt.TryGetProperty("data", out var ch))
                    OnChannelCreate?.Invoke(this, new(
                        ch.GetProperty("id").GetString() ?? string.Empty,
                        ch.GetProperty("name").GetString() ?? string.Empty,
                        (ChannelType)ch.GetProperty("type").GetInt32()
                    ));

                return;
            }
            case "VOICE_CHANNEL_SELECT": {
                if (evt.TryGetProperty("data", out var vc))
                    OnSelectedVoiceChannel?.Invoke(this, new(
                        vc.GetProperty("channel_id").GetString() ?? string.Empty,
                        vc.GetProperty("guild_id").GetString() ?? string.Empty
                    ));

                return;
            }
            case "VOICE_SETTINGS_UPDATE": {
                if (evt.TryGetProperty("data", out var vs) &&
                    vs.TryGetProperty("mode", out var vsMode))
                    OnVoiceSettingsUpdate?.Invoke(this, new(
                        vs.GetProperty("input").GetProperty("volume").GetSingle(),
                        vs.GetProperty("output").GetProperty("volume").GetSingle(),
                        vsMode.GetProperty("type").GetString() ?? string.Empty,
                        vsMode.GetProperty("auto_threshold").GetBoolean(),
                        vsMode.GetProperty("threshold").GetSingle(),
                        vsMode.GetProperty("delay").GetSingle(),
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
            case "VOICE_STATE_CREATE": {
                if (evt.TryGetProperty("data", out var vsd) &&
                    vsd.TryGetProperty("user", out var usr) &&
                    vsd.TryGetProperty("voice_state", out var vsdvs))
                    OnVoiceStateCreate?.Invoke(this, new(
                        usr.GetProperty("id").GetString() ?? string.Empty,
                        usr.GetProperty("username").GetString() ?? string.Empty,
                        vsd.GetProperty("nick").GetString() ?? string.Empty,
                        usr.GetProperty("bot").GetBoolean(),
                        vsd.GetProperty("volume").GetInt32(),
                        vsd.GetProperty("mute").GetBoolean(),
                        vsdvs.GetProperty("mute").GetBoolean(),
                        vsdvs.GetProperty("deaf").GetBoolean(),
                        vsdvs.GetProperty("self_mute").GetBoolean(),
                        vsdvs.GetProperty("self_deaf").GetBoolean(),
                        vsdvs.GetProperty("suppress").GetBoolean()
                    ));

                return;
            }
            case "VOICE_STATE_UPDATE": {
                if (evt.TryGetProperty("data", out var vsd) &&
                    vsd.TryGetProperty("user", out var usr) &&
                    vsd.TryGetProperty("voice_state", out var vsdvs))
                    OnVoiceStateUpdate?.Invoke(this, new(
                        usr.GetProperty("id").GetString() ?? string.Empty,
                        usr.GetProperty("username").GetString() ?? string.Empty,
                        vsd.GetProperty("nick").GetString() ?? string.Empty,
                        usr.GetProperty("bot").GetBoolean(),
                        vsd.GetProperty("volume").GetInt32(),
                        vsd.GetProperty("mute").GetBoolean(),
                        vsdvs.GetProperty("mute").GetBoolean(),
                        vsdvs.GetProperty("deaf").GetBoolean(),
                        vsdvs.GetProperty("self_mute").GetBoolean(),
                        vsdvs.GetProperty("self_deaf").GetBoolean(),
                        vsdvs.GetProperty("suppress").GetBoolean()
                    ));

                return;
            }
            case "VOICE_STATE_DELETE": {
                if (evt.TryGetProperty("data", out var vsd) &&
                    vsd.TryGetProperty("user", out var usr) &&
                    vsd.TryGetProperty("voice_state", out var vsdvs))
                    OnVoiceStateDelete?.Invoke(this, new(
                        usr.GetProperty("id").GetString() ?? string.Empty,
                        usr.GetProperty("username").GetString() ?? string.Empty,
                        vsd.GetProperty("nick").GetString() ?? string.Empty,
                        usr.GetProperty("bot").GetBoolean(),
                        vsd.GetProperty("volume").GetInt32(),
                        vsd.GetProperty("mute").GetBoolean(),
                        vsdvs.GetProperty("mute").GetBoolean(),
                        vsdvs.GetProperty("deaf").GetBoolean(),
                        vsdvs.GetProperty("self_mute").GetBoolean(),
                        vsdvs.GetProperty("self_deaf").GetBoolean(),
                        vsdvs.GetProperty("suppress").GetBoolean()
                    ));

                return;
            }
            case "VOICE_CONNECTION_STATUS": {
                if (evt.TryGetProperty("data", out var vcstat)) {
                    string state = vcstat.GetProperty("state").GetString() ?? string.Empty;
                    OnVoiceStatusUpdate?.Invoke(this, new(state, VoiceStateToInt(state)));
                }

                return;
            }
            case "MESSAGE_CREATE": {
                if (evt.TryGetProperty("data", out var msgd) &&
                    msgd.TryGetProperty("message", out var msg) &&
                    msg.TryGetProperty("author", out var msga))
                    OnMessageCreate?.Invoke(this, new(
                        msgd.GetProperty("channel_id").GetString() ?? string.Empty,
                        msg.GetProperty("id").GetString() ?? string.Empty,
                        msg.GetProperty("blocked").GetBoolean(),
                        msg.GetProperty("content").GetString() ?? string.Empty,
                        msg.GetProperty("timestamp").GetString() ?? string.Empty,
                        msg.GetProperty("edited_timestamp").GetString() ?? string.Empty,
                        msg.GetProperty("pinned").GetBoolean(),
                        msga.GetProperty("id").GetString() ?? string.Empty,
                        msga.GetProperty("username").GetString() ?? string.Empty,
                        msga.GetProperty("bot").GetBoolean()
                    ));

                return;
            }
            case "MESSAGE_UPDATE": {
                if (evt.TryGetProperty("data", out var msgd) &&
                    msgd.TryGetProperty("message", out var msg) &&
                    msg.TryGetProperty("author", out var msga))
                    OnMessageUpdate?.Invoke(this, new(
                        msgd.GetProperty("channel_id").GetString() ?? string.Empty,
                        msg.GetProperty("id").GetString() ?? string.Empty,
                        msg.GetProperty("blocked").GetBoolean(),
                        msg.GetProperty("content").GetString() ?? string.Empty,
                        msg.GetProperty("timestamp").GetString() ?? string.Empty,
                        msg.GetProperty("edited_timestamp").GetString() ?? string.Empty,
                        msg.GetProperty("pinned").GetBoolean(),
                        msga.GetProperty("id").GetString() ?? string.Empty,
                        msga.GetProperty("username").GetString() ?? string.Empty,
                        msga.GetProperty("bot").GetBoolean()
                    ));

                return;
            }
            case "MESSAGE_DELETE": {
                if (evt.TryGetProperty("data", out var msgd) &&
                    msgd.TryGetProperty("message", out var msg) &&
                    msg.TryGetProperty("author", out var msga))
                    OnMessageDelete?.Invoke(this, new(
                        msgd.GetProperty("channel_id").GetString() ?? string.Empty,
                        msg.GetProperty("id").GetString() ?? string.Empty,
                        msg.GetProperty("blocked").GetBoolean(),
                        msg.GetProperty("content").GetString() ?? string.Empty,
                        msg.GetProperty("timestamp").GetString() ?? string.Empty,
                        msg.GetProperty("edited_timestamp").GetString() ?? string.Empty,
                        msg.GetProperty("pinned").GetBoolean(),
                        msga.GetProperty("id").GetString() ?? string.Empty,
                        msga.GetProperty("username").GetString() ?? string.Empty,
                        msga.GetProperty("bot").GetBoolean()
                    ));

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
            case "NOTIFICATION_CREATE": {
                if (evt.TryGetProperty("data", out var notivd) &&
                    notivd.TryGetProperty("message", out var msg) &&
                    msg.TryGetProperty("author", out var msga))
                    OnNotificationCreate?.Invoke(this, new(
                        notivd.GetProperty("channel_id").GetString() ?? string.Empty,
                        notivd.GetProperty("title").GetString() ?? string.Empty,
                        notivd.GetProperty("body").GetString() ?? string.Empty,
                        notivd.GetProperty("icon_url").GetString() ?? string.Empty,
                        msg.GetProperty("id").GetString() ?? string.Empty,
                        msg.GetProperty("blocked").GetBoolean(),
                        msg.GetProperty("content").GetString() ?? string.Empty,
                        msg.GetProperty("timestamp").GetString() ?? string.Empty,
                        msg.GetProperty("edited_timestamp").GetString() ?? string.Empty,
                        msg.GetProperty("pinned").GetBoolean(),
                        msga.GetProperty("id").GetString() ?? string.Empty,
                        msga.GetProperty("username").GetString() ?? string.Empty,
                        msga.GetProperty("bot").GetBoolean()
                    ));

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
