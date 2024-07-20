using AkkaClusterWebApi.App.Hubs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;

namespace AkkaClusterWebApi.App.Controllers;

[ApiController]
[Route("RemoteDevices")]
public class DeviceController : ControllerBase
{

    private readonly IConnectPropagator _device;

    public DeviceController(IConnectPropagator device)
    {
        _device = device;
    }

    [HttpPost]
    [Route("start")]
    public Task<IActionResult> StartGeneratingStream([FromBody]ClientConnectionId connectionId, CancellationToken token = default)
    {
        _device.StartOrResumeStreaming(connectionId.EscapedConnectionId);
        IActionResult result = Ok(connectionId);
        return Task.FromResult(result);
    }
    
    [HttpPost]
    [Route("stop")]
    public Task<IActionResult> StopGeneratingStream([FromBody]ClientConnectionId connectionId, CancellationToken token = default)
    {
        _device.StopStreaming(connectionId.EscapedConnectionId);
        IActionResult result = Ok(connectionId);
        return Task.FromResult(result);
    }

    private Task<IActionResult>  GetMethods([FromServices]IHubContext<RemoteDeviceHub> context, CancellationToken token = default)
    {
       // var mts = context.
        IActionResult result = Ok("");
        return Task.FromResult(result);
    }

    public record ClientConnectionId(string EscapedConnectionId);
}