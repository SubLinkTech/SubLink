using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Threading.Tasks;
using xyz.yewnyx.SubLink.Discord.Client;

namespace xyz.yewnyx.SubLink.Discord.Services;

[UsedImplicitly]
internal sealed partial class DiscordService {
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IOptionsMonitor<DiscordSettings> _settingsMonitor;
    private DiscordSettings _settings;
    private readonly DiscordClient _discord;
    private DiscordAuth? _auth;
    private string _accessToken = string.Empty;

    private readonly DiscordRules _rules;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Shhh")]
    private IServiceScope? _discordLoggedInScope;

    public DiscordService(
        ILogger logger,
        IHostApplicationLifetime applicationLifetime,
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<DiscordSettings> settingsMonitor,
        DiscordRules rules) {
        _logger = logger;
        _applicationLifetime = applicationLifetime;
        _serviceScopeFactory = serviceScopeFactory;
        _settingsMonitor = settingsMonitor;
        _settingsMonitor.OnChange(UpdateDiscordSettings);
        _settings = _settingsMonitor.CurrentValue;

        _rules = rules;

        _discord = new(_logger);
        _discord.OnNeedsRestart += Discord_OnNeedsRestart;

        WireCallbacks();
    }

    private void Discord_OnNeedsRestart(object? sender, EventArgs e) {
        try {
            BuildAndAuthorizeIPC().Wait();
        } catch (Exception ex) {
            _logger.Error("[{TAG}] Error during restart: {ERROR}", Platform.PlatformName, ex.Message);
        }
    }

    private void UpdateDiscordSettings(DiscordSettings settings) =>
        _settings = settings;

    public async Task StartAsync() {
        if (!_settings.Enabled) {
            _logger.Warning("[{TAG}] Disabled in config, skipping", Platform.PlatformName);
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.ClientID) ||
            string.IsNullOrWhiteSpace(_settings.ClientSecret)) {
            _logger.Warning("[{TAG}] Invalid config, skipping", Platform.PlatformName);
            return;
        }

        try {
            _auth = new(_settings.ClientID, _settings.ClientSecret);
            _accessToken = await _auth.FetchAccessTokenAsync();
            _logger.Debug("[{TAG}] Access token retrieved successfully.", Platform.PlatformName);

            await BuildAndAuthorizeIPC();
        } catch (Exception ex) {
            _logger.Error("[{TAG}] Error during start: {ERROR}", Platform.PlatformName, ex.Message);
        }
    }

    public async Task StopAsync() {
        _discord.Dispose();
        await Task.CompletedTask;
    }

    private async Task BuildAndAuthorizeIPC() {
        try {
            // Attempt to connect to any available Discord IPC pipe
            bool connected = false;
            _logger.Information("[{TAG}] Attempting to connect to Discord...", Platform.PlatformName);

            for (int i = 0; i < 10; i++) { // Attempt discord-ipc-0 through discord-ipc-9
                string pipeName = $"discord-ipc-{i}";
                _logger.Debug("[{TAG}] Attempting to connect to {pipeName}...", Platform.PlatformName, pipeName);
                connected = await _discord.Connect(pipeName);

                if (connected) break;
            }

            if (!connected) {
                _logger.Information("[{TAG}] Failed to connect to Discord IPC pipes. Make sure Discord is running.", Platform.PlatformName);
                return;
            }

            var handshakeResult = await _discord.Handshake(_settings.ClientID);

            if (string.IsNullOrEmpty(handshakeResult)) {
                _logger.Warning("[{TAG}] Handshake timed out. Try restarting discord.", Platform.PlatformName);
                return;
            }

            _logger.Debug("[{TAG}] Handshake completed successfully.", Platform.PlatformName);

            if (await _discord.Authenticate(_accessToken)) {
                _logger.Information("[{TAG}] Authenticated successfully!", Platform.PlatformName);
                // Subscribe to common events
                //_discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("READY"));
                //_discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("ERROR"));
                // ^ No longer needed, These are dispatched by default ^

                if (!string.IsNullOrEmpty(_settings.DefaultGuildId))
                    _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("GUILD_STATUS", new { guild_id = _settings.DefaultGuildId }));

                _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("GUILD_CREATE"));
                _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("CHANNEL_CREATE"));
                _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("VOICE_CHANNEL_SELECT"));
                _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("VOICE_SETTINGS_UPDATE"));
                _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("VOICE_CONNECTION_STATUS"));

                if (!string.IsNullOrEmpty(_settings.DefaultChannelId)) {
                    var chArgs = new { channel_id = _settings.DefaultChannelId };
                    _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("VOICE_STATE_CREATE", chArgs));
                    _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("VOICE_STATE_UPDATE", chArgs));
                    _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("VOICE_STATE_DELETE", chArgs));
                    _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("MESSAGE_CREATE", chArgs));
                    _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("MESSAGE_UPDATE", chArgs));
                    _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("MESSAGE_DELETE", chArgs));
                    _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("SPEAKING_START", chArgs));
                    _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("SPEAKING_STOP", chArgs));
                }

                _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("NOTIFICATION_CREATE"));
                _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("ACTIVITY_JOIN"));
                _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("ACTIVITY_SPECTATE"));
                _discord.SendDataAndWait(1, DiscordIpcMessage.Subscribe("ACTIVITY_JOIN_REQUEST"));

                _discord.StartListening();
            } else {
                _logger.Error("[{TAG}] Authentication timed out. Try restarting discord.", Platform.PlatformName);
                return;
            }

            _logger.Debug("[{TAG}] Discord Platform started!", Platform.PlatformName);
            _discordLoggedInScope = _serviceScopeFactory.CreateScope();
            _rules.SetService(this);
            Task.Run(async () => {
                await Task.Delay(TimeSpan.FromSeconds(2));
                _discord.FireOnReadyEvent();
            });
            return;
        } catch (Exception ex) {
            _logger.Error("[{TAG}] Error during start: {ERROR}", Platform.PlatformName, ex.Message);
            return;
        }
    }
}
