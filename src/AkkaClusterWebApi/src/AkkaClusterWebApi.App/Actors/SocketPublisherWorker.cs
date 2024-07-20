using System.Security.Policy;
using AkaClusterConsole.Actors.Contracts;
using AkaClusterConsole.Dtos.Message;
using Akka.Actor;
using Akka.DependencyInjection;
using Akka.Event;
using Akka.Streams;
using Akka.Streams.Dsl;
using Akka.Streams.SignalR.AspNetCore;
using AkkaClusterWebApi.App.Hubs;
namespace AkkaClusterWebApi.App.Actors;


public record ConnectClient(string ConnectionId);
public class SocketPublisherManager : ReceiveActor
{ 
    public static class Messages
    {
        public record StopSendingMessages(string ConnectionId); 
        public record StartSendingMessages(string ConnectionId);

        public record RemoteClientDisconnected(ClientConnectedToWebSocket PrevConnectionDetails);
    }
    protected IDependencyResolver Resolver { get; } = DependencyResolver.For(Context.System).Resolver;
    private static readonly SocketPublisherWorker.Messages.StopPeriodicApplicantCreatedFakeMessages _stop = new();
    private static readonly SocketPublisherWorker.Messages.StartSendingMessages _start = new();
    protected readonly ILoggingAdapter _logger = Context.GetLogger();
    public SocketPublisherManager()
    {
        Receive<ConnectClient>(HandleConnectClient);
        Receive<Messages.StopSendingMessages>(HandleStop);
        Receive<Messages.StartSendingMessages>(HandleStart);
        Receive<ClientConnectedToWebSocket>(HandleClientConnectedToWebSocket);
        Receive<Messages.RemoteClientDisconnected>(HandleRemoteClientDisconnected);
    }

    private void HandleRemoteClientDisconnected(Messages.RemoteClientDisconnected obj)
    {
        var childRef = Context.Child(obj.PrevConnectionDetails.ConnectionIdDataEscaped);
        childRef?.Tell(_stop);
        Context.Stop(childRef);
        
    }

    private void HandleClientConnectedToWebSocket(ClientConnectedToWebSocket child)
    {
        try
        {
            _logger.Info("actor with info is being created: {0}", child);
            Context.Child(child.ConnectionIdDataEscaped).GetOrElse(() =>
            {
                // var service = Resolver.GetService<IRemoteDeviceHubService>();
                var props = Resolver.Props<SocketPublisherWorker>(child.ConnectionIdDataEscaped);
                // var props = Akka.Actor.Props.Create<SocketPublisherWorker>(new SocketPublisherWorker(service, child))
                return Context.ActorOf(props, child.ConnectionIdDataEscaped);
            });
        }
        catch (Exception e)
        {
            _logger.Error("error while creating work child: {0}", e);
        }
    }

    private void HandleStop(Messages.StopSendingMessages obj)
    {
        var childRef = Context.Child(Uri.EscapeDataString(obj.ConnectionId));
        childRef?.Tell(_stop);
    } 
    
    private void HandleStart(Messages.StartSendingMessages obj)
    {
        var childRef = Context.Child(Uri.EscapeDataString(obj.ConnectionId));
        childRef?.Tell(_start);
    }

    private void HandleConnectClient(ConnectClient command)
    {
        
    }
}

public class SocketPublisherWorker : ReceiveActor , IWithTimers
{
    public static class Messages
    {
        public sealed record StartSendingMessages();

        public sealed record FrameReceived(byte[] Data, DateTimeOffset ? When = null);

        public record SendPeriodicFakeApplicantCReatedMessagesToHubClients();
            
        public record StartPeriodicFakeApplicantCReatedMessagesToHubClients();

        public record StopPeriodicApplicantCreatedFakeMessages();
    }

    private readonly IRemoteDeviceHubService _service;
    private readonly string _connectionId;
    private IActorRef _sendTo;
    private const string TimeId = "send_to_stream_signalr";
    protected readonly ILoggingAdapter _logger = Context.GetLogger();
    private readonly HubService _hub;
    public SocketPublisherWorker(IRemoteDeviceHubService service, HubService hub , string connectionId)
    {
        _connectionId = connectionId;
        _service = service;
        _hub = hub;
        ReceiveAsync<Messages.SendPeriodicFakeApplicantCReatedMessagesToHubClients>(Handle);
        Receive<Messages.StopPeriodicApplicantCreatedFakeMessages>(HandleStop);
        Receive<Messages.StartSendingMessages>(HandleStart);
    }

    private void HandleStart(Messages.StartSendingMessages command)
    {
        Timers.StartPeriodicTimer(TimeId, new Messages.SendPeriodicFakeApplicantCReatedMessagesToHubClients() ,TimeSpan.FromSeconds(3));
        _logger.Info("starting sending messages to stream");
    }

    private void HandleStop(Messages.StopPeriodicApplicantCreatedFakeMessages obj)
    {
        _logger.Info("stopping sending messages to stream");
        Timers.CancelAll();
    }

    private async Task Handle(Messages.SendPeriodicFakeApplicantCReatedMessagesToHubClients reminder)
    {
        try
        {
            var next = new ApplicantCreated()
            {
                Id = Guid.NewGuid(), When = DateTimeOffset.Now
            };
            // _logger.Info("socket client is not there(i.e no socket client)");
            await _hub.TellThem(next);


            _logger.Info("client is = ({0}) sending message to the socket", _connectionId);
        }
        catch (Exception e)
        {
            _logger.Error("error send events to clients via hub: from worker: {0}", e);
        }
    }

    protected void CreateGraph()
    {
        
        var actorSource = Source.ActorRef<ApplicantCreated>(300, OverflowStrategy.DropHead);
        var (preMatActor, preMatSource) = actorSource.PreMaterialize(Context.Materializer());
        var flow = Flow.Create<ApplicantCreated>()
            .Collect(x => x != null, response => response)
            .Select(x => Signals.SendToGroup("Received", x));
        preMatSource.Via(flow).RunWith(_service.Outbound, Context.Materializer());
        _sendTo = preMatActor;
        
    }
    protected override void PreStart()
    {
        try
        {
            // CreateGraph(); 
            Timers.StartPeriodicTimer(TimeId, new Messages.SendPeriodicFakeApplicantCReatedMessagesToHubClients(),
                TimeSpan.FromSeconds(3));
            base.PreStart();
        }
        catch (Exception e)
        {
            _logger.Error("error on startup, maybe socket graph is not built yet: {0}", e);
        }
    }

    public ITimerScheduler Timers { get; set; }
}