using BuildSoft.OscCore;
using JetBrains.Annotations;
using Serilog;
using VRC.OSCQuery;
using static VRC.OSCQuery.Extensions;

namespace xyz.yewnyx.SubLink.Services;

[UsedImplicitly]
public sealed class OSCSupportService(ILogger logger, ScriptGlobals globals)
{
    public int OSCPort { get; private set; }
    public int OSCQueryPort { get; private set; }

    private readonly ILogger _logger = logger;
    private readonly ScriptGlobals _globals = globals;
    private readonly MeaModDiscovery _discovery = new();

    public void Start() {
        OSCPort = GetAvailableUdpPort();
        OSCQueryPort = GetAvailableTcpPort();

        _logger.Debug("Setting up OSCQuery on ports UDP:{OSCPort} TCP:{OSCQueryPort}", OSCPort, OSCQueryPort);

        _discovery.OnOscServiceAdded += profile => {
            _logger.Information("OSC Service Added: {Name} on port {Port}", profile.name, profile.port);
        };

        _globals.oscServer = OscServer.GetOrCreate(OSCPort);
        _globals.oscQuery = new OSCQueryServiceBuilder()
            .WithServiceName("SubLink")
            .WithUdpPort(OSCPort)
            .WithTcpPort(OSCQueryPort)
            .WithDiscovery(_discovery)
            .StartHttpServer()
            .AdvertiseOSC()
            .AdvertiseOSCQuery()
            .Build();
        _globals.oscQuery.RefreshServices();

        /*
        _globals.oscQuery.AddEndpoint<bool>("/avatar/parameters/MuteSelf", Attributes.AccessValues.ReadWrite, new object[] { true }); 
        _globals.oscServer.TryAddMethod("/avatar/parameters/MuteSelf", message => {
            _logger.Information($"Received `/avatar/parameters/MuteSelf` value from VRChat : {message.ReadBooleanElement(0)}");
        });
        */
    }
}
