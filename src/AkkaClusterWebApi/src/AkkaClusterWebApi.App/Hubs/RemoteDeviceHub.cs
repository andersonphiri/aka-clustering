using Akka;
using Akka.Streams.Dsl;
using Akka.Streams.SignalR.AspNetCore;
using Akka.Streams.SignalR.AspNetCore.Internals;
using Microsoft.AspNetCore.SignalR;

namespace AkkaClusterWebApi.App.Hubs;

public class RemoteDeviceHub : StreamHub<RemoteDeviceStream>
{
    public RemoteDeviceHub(IStreamDispatcher dispatcher) : base(dispatcher)
    {
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