using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using xyz.yewnyx.SubLink.Kick.KickClient;
using xyz.yewnyx.SubLink.Kick.Services;
using xyz.yewnyx.SubLink.Platforms;

namespace xyz.yewnyx.SubLink.Kick;

public class Platform : IPlatform {
    internal const string PlatformName = "Kick";
    internal static string PlatformConfigFile = GlobalUtils.GetSettingsFilePath(PlatformName);

#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable IDE1006 // Naming Styles
    private ILogger? _logger { get; set; }
    private IServiceProvider? _serviceProvider { get; set; }
    private KickService? _service { get; set; }
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0052 // Remove unread private members

    // Useful for enumerating the loaded platforms
    public string GetPlatformName() => PlatformName;
    public string GetServiceSymbol() => "SUBLINK_KICK";
    public string[] GetAdditionalUsings() => [
        "xyz.yewnyx.SubLink.Kick.Services",
        "xyz.yewnyx.SubLink.Kick.KickClient",
    ];
    public string[] GetAdditionalAssemblies() => [];

    public bool EnsureConfigExists() {
        if (!File.Exists(PlatformConfigFile)) {
            File.WriteAllText(PlatformConfigFile, """
{
  "Kick": {
    "PusherKey": "",
    "PusherCluster": "",
    "ChatroomId": ""
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
            .Configure<KickSettings>(context.Configuration.GetSection("Kick"))
            .AddSingleton<KickPusherClient>()
            .AddScoped<KickRules>()
            .AddScoped<KickService>();

    public void AppendRules(Dictionary<string, IPlatformRules> rules) {
        var rulesSvc = _serviceProvider?.GetService<KickRules>();

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

        _service = _serviceProvider?.GetService<KickService>();

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
