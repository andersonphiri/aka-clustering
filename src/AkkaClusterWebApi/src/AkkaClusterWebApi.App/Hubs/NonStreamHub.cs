using AkaClusterConsole.Dtos.Message;
using Microsoft.AspNetCore.SignalR;

namespace AkkaClusterWebApi.App.Hubs;

public interface INonStreamHub
{
    [HubMethodName("Received")]
    Task SendDataAsync(object data, CancellationToken token = default);
}

public class NonStreamHub : Hub<INonStreamHub>
{
    public const string HubUrl = "/hubs/nonstream/devicehub"; 
    private readonly IEventPublisher _publisher;
    private readonly IConnectPropagator _propagator;
    private readonly ILogger<NonStreamHub> _logger;

    public NonStreamHub(IConnectPropagator propagator, IEventPublisher publisher, ILogger<NonStreamHub> logger)
    {
        _propagator = propagator;
        _publisher = publisher;
        _logger = logger;
    }

    public override async Task OnDisconnectedAsync(Exception? exception)
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
            _propagator.NotifyClientDisconnected(connected);
            await base.OnDisconnectedAsync(exception);
            _logger.LogInformation("client has disconnected with error: {c} {e}",connected, exception);
           
        }
        catch (Exception e)
        {
            _logger.LogError("error handling socket client disconnected (from non streaming hub) with info: {e} :{n} {ex}", connected, Environment.NewLine,e);
        }
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
            _logger.LogError("error handling socket client(from non streaming h ub) with info: {e} :{n} {ex}", connected, Environment.NewLine,e);
        }
    }

    
}

public class HubService
{
    private readonly IHubContext<NonStreamHub, INonStreamHub> _hub;
    private readonly ILogger<HubService> _logger;
    public HubService(IHubContext<NonStreamHub, INonStreamHub> hub, ILogger<HubService>  logger)
    {
        _logger = logger;
        _hub = hub;
    }
    

    
    public async Task TellThem(object @object, CancellationToken token = default)
    {
        try
        {
            await _hub.Clients.All.SendDataAsync(@object, token);
        }
        catch (Exception e)
        {
            _logger.LogError("error sending data to clients over non streaming channel: {e}", e);
        }
    }
}