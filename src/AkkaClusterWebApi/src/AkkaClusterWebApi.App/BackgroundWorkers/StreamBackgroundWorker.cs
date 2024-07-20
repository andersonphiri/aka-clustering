using AkaClusterConsole.Dtos.Message;
using Akka.Actor;
using Akka.Hosting;
using AkkaClusterWebApi.App.Actors;
using AkkaClusterWebApi.App.Hubs;

namespace AkkaClusterWebApi.App.BackgroundWorkers;

public class StreamBackgroundWorker : BackgroundService,  IConnectPropagator, IEventPublisher
{
    private IActorRef _socketPublisherManager = null;
    private readonly ActorSystem _system;
    private readonly ILogger<StreamBackgroundWorker> _logger;
    private readonly IHostApplicationLifetime _applicationLifetime;

    public StreamBackgroundWorker(ActorSystem system, ILogger<StreamBackgroundWorker> logger, IHostApplicationLifetime applicationLifetime)
    {
        _logger = logger;
        _system = system;
        _applicationLifetime = applicationLifetime;
    }
    
    public void LaunchStreamToClient(ClientConnectedToWebSocket connectionIdInfo)
    {
        _socketPublisherManager.Tell(connectionIdInfo);
    }

    public void StopStreaming(string connectionIdEscaped)
    {
        _socketPublisherManager.Tell(new SocketPublisherManager.Messages.StopSendingMessages(connectionIdEscaped));
    }

    public void StartOrResumeStreaming(string connectionIdEscaped)
    {
        _socketPublisherManager.Tell(new SocketPublisherManager.Messages.StartSendingMessages(connectionIdEscaped));
    }

    public void NotifyClientDisconnected(ClientConnectedToWebSocket connectionIfo)
    {
        _socketPublisherManager.Tell(new SocketPublisherManager.Messages.RemoteClientDisconnected(connectionIfo));

    }

    public void Publish(object @event)
    {
        try
        {
            _logger.LogInformation("published event: {e}", @event);
            _system.EventStream.Publish(@event);
        }
        catch (Exception e)
        {
            _logger.LogError("error when publishing event: {e}", e);
        }
    }

    public ValueTask PublishAsync(object @event, CancellationToken token = default)
    {
        try
        {
            _logger.LogInformation("published event: {e}", @event);
            _system.EventStream.Publish(@event);
        }
        catch (Exception e)
        {
            _logger.LogError("error when publishing event: {e}", e);
        }

        return ValueTask.CompletedTask;
    }

    public override async Task StartAsync(CancellationToken cancellationToken)
    {
        try
        {
            _socketPublisherManager =
                _system.ActorOf(Props.Create<SocketPublisherManager>(() => new SocketPublisherManager()));
            _logger.LogInformation("service work started succesfully: {0}", _socketPublisherManager);
            await base.StartAsync(cancellationToken);
        }
        catch (Exception e)
        {
            _logger.LogError("error starting up stream background worker: {e}", e);
        }
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        try
        {
            await _system.WhenTerminated.ContinueWith(tr =>
            {
                _applicationLifetime.StopApplication();
            });
            _logger.LogInformation("global system shuts down...");
        }
        catch (Exception e)
        {
            _logger.LogError("error stopping app: {e}", e);
        }
    }
}