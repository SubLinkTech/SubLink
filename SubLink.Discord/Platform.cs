using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using xyz.yewnyx.SubLink.Discord.Services;
using xyz.yewnyx.SubLink.Platforms;

namespace xyz.yewnyx.SubLink.Discord;

public class Platform : IPlatform {
    internal const string PlatformName = "Discord";
    internal static string PlatformConfigFile = Path.Combine("settings", $"{PlatformName}.json");

#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable IDE1006 // Naming Styles
    private ILogger? _logger { get; set; }
    private IServiceProvider? _serviceProvider { get; set; }
    private DiscordService? _service { get; set; }
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0052 // Remove unread private members

    // Useful for enumerating the loaded platforms
    public string GetPlatformName() => PlatformName;
    public string GetServiceSymbol() => "SUBLINK_DISCORD";
    public string[] GetAdditionalUsings() => [
        "System.Collections.Immutable",
        "xyz.yewnyx.SubLink.Discord.Services"
    ];
    public string[] GetAdditionalAssemblies() => [];

    public bool EnsureConfigExists() {
        if (!File.Exists(PlatformConfigFile)) {
            File.WriteAllText(PlatformConfigFile, """
{
  "Discord": {
    "Enabled": false,
    "ClientID": "1316146450681303071",
    "ClientSecret": "cTZvtl89suuCa41EaGu8MFrhDfagtt_5",
    "DefaultGuildId": "",
    "DefaultChannelId": ""
  }
}
""");
            return false;
        }

        return true;
    }

    public void ConfigureAppConfiguration(HostBuilderContext context, IConfigurationBuilder builder) =>
        builder.AddJsonFile(PlatformConfigFile, false, true);

    public void ConfigureServices(HostBuilderContext context, IServiceCollection services) =>
        services
            .Configure<DiscordSettings>(context.Configuration.GetSection("Discord"))
            .AddScoped<DiscordRules>()
            .AddScoped<DiscordService>();

    public void AppendRules(Dictionary<string, IPlatformRules> rules) {
        var rulesSvc = _serviceProvider?.GetService<DiscordRules>();

        if (rulesSvc != null)
            rules.Add(PlatformName, rulesSvc);
    }

    public void SetLogger(ILogger logger) =>
        _logger = logger;

    public void SetServiceProvider(IServiceProvider serviceProvider) =>
        _serviceProvider = serviceProvider;

    // Let the interface handle this, no reflection overhead
    public async Task StartServiceAsync() {
        if (_service != null)
            await _service.StopAsync();

        _service = _serviceProvider?.GetService<DiscordService>();

        if (_service != null)
            await _service.StartAsync();
    }

    public async Task StopServiceAsync() {
        if (_service == null)
            return;

        await _service.StopAsync();
        _service = null;
    }
}
