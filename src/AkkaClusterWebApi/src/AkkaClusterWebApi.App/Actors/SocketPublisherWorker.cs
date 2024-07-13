using Akka.Actor;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.SignalR.AspNetCore;
using AkkaClusterWebApi.App.Hubs;
namespace AkkaClusterWebApi.App.Actors;

public class SocketPublisherWorker : ReceiveActor
{
    public static class Messages
    {
        public sealed record StartSendingMessages();

        public sealed record FrameReceived(byte[] Data, DateTimeOffset ? When = null);

        public record StartPeriodicTimerMessagesToHubCLients();
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