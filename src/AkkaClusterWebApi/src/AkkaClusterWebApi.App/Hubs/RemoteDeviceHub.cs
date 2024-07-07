using Akka;
using Akka.Actor;
using Akka.Streams;
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
    public RemoteDeviceStream(IHubClients clients, ConnectionSourceSettings sourceSettings = null, ConnectionSinkSettings sinkSettings = null) : base(clients, sourceSettings, sinkSettings)
    {
    }
}

public interface IRemoteDeviceHubService
{
    bool HasValue { get; }
    Source<ISignalREvent, NotUsed> Inbound { get; }
    Sink<ISignalRResult, NotUsed> Outbound { get; }
}

public sealed class RemoteDeviceHubService : IRemoteDeviceHubService
{
    public bool HasValue { get; private set; }
    public Source<ISignalREvent, NotUsed> Inbound { get; private set; } 
    public Sink<ISignalRResult, NotUsed> Outbound { get; private set; }

    public RemoteDeviceHubService(Source<ISignalREvent, NotUsed>  source, Sink<ISignalRResult, NotUsed> sink )
    {
        if (!HasValue)
        {
            Inbound = source;
            Outbound = sink;
        }
    }
}

public class SocketPublisherWorker : ReceiveActor
{
    public static class Messages
    {
        public sealed record StartSendingMessages();

        public sealed record FrameReceived(byte[] Data, DateTimeOffset ? When = null);
    }

    private readonly IRemoteDeviceHubService _service;
    private readonly string _connectionId;
    private IActorRef _sendTo;
    public SocketPublisherWorker(IRemoteDeviceHubService service, string connectionId)
    {
        _connectionId = connectionId;
        _service = service;
    }

    protected override void PreStart()
    {
        var actorSource = Source.ActorRef<Messages.FrameReceived>(300, OverflowStrategy.DropHead);
        var (preMatActor, preMatSource) = actorSource.PreMaterialize(Context.Materializer());
        var flow = Flow.Create<Messages.FrameReceived>()
            .Collect(x => x.Data != null, response => response)
            .Select(x => Signals.Send(_connectionId, x));
        preMatSource.Via(flow).RunWith(_service.Outbound, Context.Materializer());
        _sendTo = preMatActor;
        base.PreStart();
    }
}