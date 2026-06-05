using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Serilog;
using System;
using System.Threading.Tasks;
using xyz.yewnyx.SubLink.Joystick.Client;

namespace xyz.yewnyx.SubLink.Joystick.Services;

[UsedImplicitly]
internal sealed partial class JoystickService {
    private readonly ILogger _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;
    private readonly IServiceScopeFactory _serviceScopeFactory;

    // ReSharper disable once PrivateFieldCanBeConvertedToLocalVariable
    private readonly IOptionsMonitor<JoystickSettings> _settingsMonitor;
    private JoystickSettings _settings;

    private readonly JoystickClient _client;

    private readonly JoystickRules _rules;

    [System.Diagnostics.CodeAnalysis.SuppressMessage("CodeQuality", "IDE0052:Remove unread private members", Justification = "Shhh")]
    private IServiceScope? _jopystickLoggedInScope;

    public JoystickService(
        ILogger logger,
        IHostApplicationLifetime applicationLifetime,
        IServiceScopeFactory serviceScopeFactory,
        IOptionsMonitor<JoystickSettings> settingsMonitor,
        JoystickClient joystickClient,
        JoystickRules rules) {
        _logger = logger;
        _applicationLifetime = applicationLifetime;
        _serviceScopeFactory = serviceScopeFactory;
        _settingsMonitor = settingsMonitor;
        _settingsMonitor.OnChange(UpdateJoystickSettings);
        _settings = _settingsMonitor.CurrentValue;

        _client = joystickClient ?? throw new ArgumentNullException(nameof(joystickClient));

        _rules = rules;

        WireCallbacks();
    }

    private void UpdateJoystickSettings(JoystickSettings settings) => _settings = settings;

    public async Task StartAsync() {
        _client.Enabled = _settings.Enabled;

        if (!_settings.Enabled) {
            _logger.Warning("[{TAG}] Disabled in config, skipping", Platform.PlatformName);
            return;
        }

        if (string.IsNullOrWhiteSpace(_settings.ApplicationId) ||
            string.IsNullOrWhiteSpace(_settings.ClientId) ||
            string.IsNullOrWhiteSpace(_settings.ClientSecret)) {
            _logger.Warning("[{TAG}] Invalid config, skipping", Platform.PlatformName);
            return;
        }

        if (await _client.ConnectAsync(_settings)) {
            _jopystickLoggedInScope = _serviceScopeFactory.CreateScope();
            //_rules.SetService(this);
        } else {
            _logger.Warning("[{TAG}] Failed to connect to websocket", Platform.PlatformName);
            _applicationLifetime.StopApplication();
        }
    }

    public async Task StopAsync() =>
        await _client.DisconnectAsync();
}
