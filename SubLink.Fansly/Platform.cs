using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using xyz.yewnyx.SubLink.Fansly.FanslyClient;
using xyz.yewnyx.SubLink.Fansly.Services;
using xyz.yewnyx.SubLink.Platforms;

namespace xyz.yewnyx.SubLink.Fansly;

public class Platform : IPlatform {
    internal const string PlatformName = "Fansly";
    internal static string PlatformConfigFile = GlobalUtils.GetSettingsFilePath(PlatformName);

#pragma warning disable IDE0052 // Remove unread private members
#pragma warning disable IDE1006 // Naming Styles
    private ILogger? _logger { get; set; }
    private IServiceProvider? _serviceProvider { get; set; }
    private FanslyService? _service { get; set; }
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0052 // Remove unread private members

    // Useful for enumerating the loaded platforms
    public string GetPlatformName() => PlatformName;
    public string GetServiceSymbol() => "SUBLINK_FANSLY";
    public string[] GetAdditionalUsings() => [
        "xyz.yewnyx.SubLink.Fansly.Services",
        "xyz.yewnyx.SubLink.Fansly.FanslyClient",
    ];
    public string[] GetAdditionalAssemblies() => [];

    public bool EnsureConfigExists() {
        if (!File.Exists(PlatformConfigFile)) {
            File.WriteAllText(PlatformConfigFile, """
{
  "Fansly": {
    "Token": "",
    "Username": ""
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
            .Configure<FanslySettings>(context.Configuration.GetSection("Fansly"))
            .AddSingleton<FanslyWSClient>()
            .AddScoped<FanslyRules>()
            .AddScoped<FanslyService>();

    public void AppendRules(Dictionary<string, IPlatformRules> rules) {
        var rulesSvc = _serviceProvider?.GetService<FanslyRules>();

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

        _service = _serviceProvider?.GetService<FanslyService>();

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
