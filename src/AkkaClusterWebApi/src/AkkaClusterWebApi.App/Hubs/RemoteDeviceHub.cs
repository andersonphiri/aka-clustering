using Akka;
using Akka.Streams.Dsl;
using Akka.Streams.SignalR.AspNetCore;
using Akka.Streams.SignalR.AspNetCore.Internals;
using Microsoft.AspNetCore.SignalR;
using AkaClusterConsole.Dtos.Message;
namespace AkkaClusterWebApi.App.Hubs;

public interface IEventPublisher
{
    void Publish(object @event);
    ValueTask PublishAsync(object @event, CancellationToken token = default);
}

public interface IConnectPropagator
{
    void LaunchStreamToClient(ClientConnectedToWebSocket connectionIdInfo);
    void StopStreaming(string connectionIdEscaped);
    void StartOrResumeStreaming(string connectionIdEscaped);

    void NotifyClientDisconnected(ClientConnectedToWebSocket connectionIfo);
}
public class RemoteDeviceHub : StreamHub<RemoteDeviceStream>
{
    public const string HubUrl = "/hubs/stream/remotedevice";
    private readonly IEventPublisher _publisher;
    private readonly IConnectPropagator _propagator;
    private readonly ILogger<RemoteDeviceHub> _logger;
    public RemoteDeviceHub(IStreamDispatcher dispatcher, IConnectPropagator propagator, IEventPublisher publisher, ILogger<RemoteDeviceHub> logger) : base(dispatcher)
    {
        _publisher = publisher;
        _logger = logger;
        _propagator = propagator;
    }

    public override async Task OnConnectedAsync()
    {
        
        ClientConnectedToWebSocket ? connected = null;
        try
        {
            await base.OnConnectedAsync();
            connected = new ClientConnectedToWebSocket()
            {
                ConnectionIdRaw = Context.ConnectionId,
                ConnectionIdDataEscaped = Uri.EscapeDataString(Context.ConnectionId)
            };
            _propagator.LaunchStreamToClient(connected);
            await _publisher.PublishAsync(connected);
           
        }
        catch (Exception e)
        {
            _logger.LogError("error handling socket client with info: {e} :{n} {ex}", connected, Environment.NewLine,e);
        }
    }
    
}

public class RemoteDeviceStream : StreamConnector
{
    public RemoteDeviceStream(IRemoteDeviceHubService remoteService, IHubClients clients, ConnectionSourceSettings sourceSettings = null, 
        ConnectionSinkSettings sinkSettings = null) 
        : base(clients, sourceSettings, sinkSettings)
    { 
        remoteService.Connect(Source,Sink);
    }
}

public interface IRemoteDeviceHubService
{
    bool HasValue { get; }
    Source<ISignalREvent, NotUsed> Inbound { get; }
    Sink<ISignalRResult, NotUsed> Outbound { get; }

    void Connect(Source<ISignalREvent, NotUsed> source, Sink<ISignalRResult, NotUsed> sink);
}

public sealed class RemoteDeviceHubService : IRemoteDeviceHubService
{
    public bool HasValue { get; private set; }
    public Source<ISignalREvent, NotUsed> Inbound { get; private set; } 
    public Sink<ISignalRResult, NotUsed> Outbound { get; private set; }

    public void Connect(Source<ISignalREvent, NotUsed>  source, Sink<ISignalRResult, NotUsed> sink )
    {
        if (!HasValue)
        {
            Inbound = source;
            Outbound = sink;
            HasValue = true;
        }
    }
}