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

    private static string GetStringValue(JsonElement el, string property) =>
        el.TryGetProperty(property, out var outStrEl)
            ? outStrEl.GetString() ?? string.Empty
            : string.Empty;

    private static bool GetBoolValue(JsonElement el, string property) =>
        el.TryGetProperty(property, out var outStrEl) && outStrEl.GetBoolean();

    private static int GetInt32Value(JsonElement el, string property) =>
        el.TryGetProperty(property, out var outStrEl)
            ? outStrEl.GetInt32()
            : 0;

    private static float GetSingleValue(JsonElement el, string property) =>
        el.TryGetProperty(property, out var outStrEl)
            ? outStrEl.GetSingle()
            : 0f;

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
                        GetSingleValue(vs.GetProperty("input"), "volume"),
                        GetSingleValue(vs.GetProperty("output"), "volume"),
                        GetStringValue(vsMode, "type"),
                        GetBoolValue(vsMode, "auto_threshold"),
                        GetSingleValue(vsMode, "threshold"),
                        GetSingleValue(vsMode, "delay"),
                        GetBoolValue(vs, "automatic_gain_control"),
                        GetBoolValue(vs, "echo_cancellation"),
                        GetBoolValue(vs, "noise_suppression"),
                        GetBoolValue(vs, "qos"),
                        GetBoolValue(vs, "silence_warning"),
                        GetBoolValue(vs, "deaf"),
                        GetBoolValue(vs, "mute")
                    ));

                return;
            }
            case "GET_GUILD": {
                if (resp.TryGetProperty("data", out var gs))
                    OnGuildStatus?.Invoke(this, new(
                        GetStringValue(gs, "id"),
                        GetStringValue(gs, "name"),
                        GetStringValue(gs, "icon_url")
                    ));

                return;
            }
            case "GET_CHANNEL":
            case "GET_SELECTED_VOICE_CHANNEL": {
                if (resp.TryGetProperty("data", out var vc))
                    OnSelectedVoiceChannel?.Invoke(this, new(
                        GetStringValue(vc, "id"),
                        GetStringValue(vc, "guild_id")
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
                        GetInt32Value(err, "code"),
                        GetStringValue(err, "message")
                    ));

                return;
            }
            case "GUILD_STATUS": {
                if (evt.TryGetProperty("data", out var gs) && gs.TryGetProperty("guild", out var g))
                    OnGuildStatus?.Invoke(this, new(
                        GetStringValue(g, "id"),
                        GetStringValue(g, "name"),
                        GetStringValue(g, "icon_url")
                    ));

                return;
            }
            case "GUILD_CREATE": {
                if (evt.TryGetProperty("data", out var gc))
                    OnGuildCreate?.Invoke(this, new(
                        GetStringValue(gc, "id"),
                        GetStringValue(gc, "name"),
                        string.Empty
                    ));

                return;
            }
            case "CHANNEL_CREATE": {
                if (evt.TryGetProperty("data", out var ch))
                    OnChannelCreate?.Invoke(this, new(
                        GetStringValue(ch, "id"),
                        GetStringValue(ch, "name"),
                        (ChannelType)GetInt32Value(ch, "type")
                    ));

                return;
            }
            case "VOICE_CHANNEL_SELECT": {
                if (evt.TryGetProperty("data", out var vc))
                    OnSelectedVoiceChannel?.Invoke(this, new(
                        GetStringValue(vc, "channel_id"),
                        GetStringValue(vc, "guild_id")
                    ));

                return;
            }
            case "VOICE_SETTINGS_UPDATE": {
                if (evt.TryGetProperty("data", out var vs) &&
                    vs.TryGetProperty("mode", out var vsMode))
                    OnVoiceSettingsUpdate?.Invoke(this, new(
                        GetSingleValue(vs.GetProperty("input"), "volume"),
                        GetSingleValue(vs.GetProperty("output"), "volume"),
                        GetStringValue(vsMode, "type"),
                        GetBoolValue(vsMode, "auto_threshold"),
                        GetSingleValue(vsMode, "threshold"),
                        GetSingleValue(vsMode, "delay"),
                        GetBoolValue(vs, "automatic_gain_control"),
                        GetBoolValue(vs, "echo_cancellation"),
                        GetBoolValue(vs, "noise_suppression"),
                        GetBoolValue(vs, "qos"),
                        GetBoolValue(vs, "silence_warning"),
                        GetBoolValue(vs, "deaf"),
                        GetBoolValue(vs, "mute")
                    ));

                return;
            }
            case "VOICE_STATE_CREATE": {
                if (evt.TryGetProperty("data", out var vsd) &&
                    vsd.TryGetProperty("user", out var usr) &&
                    vsd.TryGetProperty("voice_state", out var vsdvs))
                    OnVoiceStateCreate?.Invoke(this, new(
                        GetStringValue(usr, "id"),
                        GetStringValue(usr, "username"),
                        GetStringValue(vsd, "nick"),
                        GetBoolValue(usr, "bot"),
                        GetInt32Value(vsd, "volume"),
                        GetBoolValue(vsd, "mute"),
                        GetBoolValue(vsdvs, "mute"),
                        GetBoolValue(vsdvs, "deaf"),
                        GetBoolValue(vsdvs, "self_mute"),
                        GetBoolValue(vsdvs, "self_deaf"),
                        GetBoolValue(vsdvs, "suppress")
                    ));

                return;
            }
            case "VOICE_STATE_UPDATE": {
                if (evt.TryGetProperty("data", out var vsd) &&
                    vsd.TryGetProperty("user", out var usr) &&
                    vsd.TryGetProperty("voice_state", out var vsdvs))
                    OnVoiceStateUpdate?.Invoke(this, new(
                        GetStringValue(usr, "id"),
                        GetStringValue(usr, "username"),
                        GetStringValue(vsd, "nick"),
                        GetBoolValue(usr, "bot"),
                        GetInt32Value(vsd, "volume"),
                        GetBoolValue(vsd, "mute"),
                        GetBoolValue(vsdvs, "mute"),
                        GetBoolValue(vsdvs, "deaf"),
                        GetBoolValue(vsdvs, "self_mute"),
                        GetBoolValue(vsdvs, "self_deaf"),
                        GetBoolValue(vsdvs, "suppress")
                    ));

                return;
            }
            case "VOICE_STATE_DELETE": {
                if (evt.TryGetProperty("data", out var vsd) &&
                    vsd.TryGetProperty("user", out var usr) &&
                    vsd.TryGetProperty("voice_state", out var vsdvs))
                    OnVoiceStateDelete?.Invoke(this, new(
                        GetStringValue(usr, "id"),
                        GetStringValue(usr, "username"),
                        GetStringValue(vsd, "nick"),
                        GetBoolValue(usr, "bot"),
                        GetInt32Value(vsd, "volume"),
                        GetBoolValue(vsd, "mute"),
                        GetBoolValue(vsdvs, "mute"),
                        GetBoolValue(vsdvs, "deaf"),
                        GetBoolValue(vsdvs, "self_mute"),
                        GetBoolValue(vsdvs, "self_deaf"),
                        GetBoolValue(vsdvs, "suppress")
                    ));

                return;
            }
            case "VOICE_CONNECTION_STATUS": {
                if (evt.TryGetProperty("data", out var vcstat)) {
                    string state = GetStringValue(vcstat, "state");
                    OnVoiceStatusUpdate?.Invoke(this, new(state, VoiceStateToInt(state)));
                }

                return;
            }
            case "MESSAGE_CREATE": {
                if (evt.TryGetProperty("data", out var msgd) &&
                    msgd.TryGetProperty("message", out var msg) &&
                    msg.TryGetProperty("author", out var msga)) {
                    OnMessageCreate?.Invoke(this, new(
                        GetStringValue(msgd, "channel_id"),
                        GetStringValue(msg, "id"),
                        GetBoolValue(msg, "blocked"),
                        GetStringValue(msg, "content"),
                        GetStringValue(msg, "timestamp"),
                        GetStringValue(msg, "edited_timestamp"),
                        GetBoolValue(msg, "pinned"),
                        GetStringValue(msga, "id"),
                        GetStringValue(msga, "username"),
                        GetBoolValue(msga, "bot")
                    ));
                }

                return;
            }
            case "MESSAGE_UPDATE": {
                if (evt.TryGetProperty("data", out var msgd) &&
                    msgd.TryGetProperty("message", out var msg) &&
                    msg.TryGetProperty("author", out var msga))
                    OnMessageUpdate?.Invoke(this, new(
                        GetStringValue(msgd, "channel_id"),
                        GetStringValue(msg, "id"),
                        GetBoolValue(msg, "blocked"),
                        GetStringValue(msg, "content"),
                        GetStringValue(msg, "timestamp"),
                        GetStringValue(msg, "edited_timestamp"),
                        GetBoolValue(msg, "pinned"),
                        GetStringValue(msga, "id"),
                        GetStringValue(msga, "username"),
                        GetBoolValue(msga, "bot")
                    ));

                return;
            }
            case "MESSAGE_DELETE": {
                if (evt.TryGetProperty("data", out var msgd) &&
                    msgd.TryGetProperty("message", out var msg) &&
                    msg.TryGetProperty("author", out var msga))
                    OnMessageDelete?.Invoke(this, new(
                        GetStringValue(msgd, "channel_id"),
                        GetStringValue(msg, "id"),
                        GetBoolValue(msg, "blocked"),
                        GetStringValue(msg, "content"),
                        GetStringValue(msg, "timestamp"),
                        GetStringValue(msg, "edited_timestamp"),
                        GetBoolValue(msg, "pinned"),
                        GetStringValue(msga, "id"),
                        GetStringValue(msga, "username"),
                        GetBoolValue(msga, "bot")
                    ));

                return;
            }
            case "SPEAKING_START": {
                if (evt.TryGetProperty("data", out var sp))
                    OnStartSpeaking?.Invoke(this, new(GetStringValue(sp, "user_id")));

                return;
            }
            case "SPEAKING_STOP": {
                if (evt.TryGetProperty("data", out var sp))
                    OnStopSpeaking?.Invoke(this, new(GetStringValue(sp, "user_id")));

                return;
            }
            case "NOTIFICATION_CREATE": {
                if (evt.TryGetProperty("data", out var notivd) &&
                    notivd.TryGetProperty("message", out var msg) &&
                    msg.TryGetProperty("author", out var msga))
                    OnNotificationCreate?.Invoke(this, new(
                        GetStringValue(notivd, "channel_id"),
                        GetStringValue(notivd, "title"),
                        GetStringValue(notivd, "body"),
                        GetStringValue(notivd, "icon_url"),
                        GetStringValue(msg, "id"),
                        GetStringValue(msg, "content"),
                        GetStringValue(msg, "timestamp"),
                        GetBoolValue(msg, "pinned"),
                        GetStringValue(msga, "id"),
                        GetStringValue(msga, "username"),
                        GetBoolValue(msga, "bot")
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
                if (evt.TryGetProperty("data", out var aj) && aj.TryGetProperty("user", out var aju))
                    OnActivityJoinRequest?.Invoke(this, new(GetStringValue(aju, "id")));

                return;
            }
            default: {
                _logger.Error($"Unsupported event: {JsonSerializer.Serialize(evt)}");
                return;
            }
        }
    }
}
